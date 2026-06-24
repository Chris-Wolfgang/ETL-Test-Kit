using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// An in-memory loader for testing error-handling paths. It consumes the source stream
/// like a normal loader, but can be configured to inject deterministic faults — throwing
/// at a given item, throwing after completion, or duplicating an item — so consumers can
/// exercise mid-stream failure, finalization failure, and idempotency logic without
/// hand-rolling broken fakes.
/// </summary>
/// <typeparam name="T">The type of item to load.</typeparam>
/// <remarks>
/// <para>
/// When constructed with <c>collectItems: true</c>, every loaded item — including any
/// duplicates — is accumulated and exposed via <see cref="GetCollectedItems"/>.
/// </para>
/// <para>
/// Faults are configured through the fluent <see cref="ThrowAt"/>,
/// <see cref="ThrowAfterCompletion"/>, and <see cref="DuplicateAt"/> methods, each of which
/// returns the same instance so calls can be chained. Multiple faults stack on a single
/// instance.
/// </para>
/// <para>
/// Fault indices are zero-based and refer to the position in the loaded (post-skip)
/// sequence. A configured fault fires <em>after</em>
/// <see cref="LoaderBase{TDestination,TProgress}.IncrementCurrentItemCount"/> for that item,
/// so a progress report reflects the item that caused the failure. When a
/// <see cref="ThrowAt"/> and a <see cref="DuplicateAt"/> are configured for the same index,
/// the throw takes precedence and the duplicate is not loaded. Calling <see cref="ThrowAt"/>
/// twice for the same index replaces the earlier exception (last-wins).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var loader = new FaultyLoader&lt;int&gt;(collectItems: true)
///     .ThrowAt(index: 25, new System.TimeoutException("connection lost"));
///
/// await loader.LoadAsync(extractor.ExtractAsync()); // throws reaching index 25
/// </code>
/// </example>
public class FaultyLoader<T> : LoaderBase<T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly bool _collectItems;
    private readonly List<T> _buffer = new List<T>();
    private readonly Dictionary<int, Exception> _throwAt = new Dictionary<int, Exception>();
    private readonly HashSet<int> _duplicateAt = new HashSet<int>();
    private Exception? _throwAfterCompletion;
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;



    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="FaultyLoader{T}"/>.
    /// </summary>
    /// <param name="collectItems">
    /// When <see langword="true"/>, loaded items (including duplicates) are accumulated in
    /// an internal buffer during each load operation and made available via
    /// <see cref="GetCollectedItems"/>. When <see langword="false"/>, items are consumed but
    /// not stored — <see cref="GetCollectedItems"/> returns <see langword="null"/>.
    /// </param>
    public FaultyLoader(bool collectItems)
    {
        _collectItems = collectItems;
    }



    /// <summary>
    /// Initializes a new <see cref="FaultyLoader{T}"/> with the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks.
    /// </summary>
    /// <param name="collectItems">
    /// When <see langword="true"/>, loaded items are accumulated and accessible via
    /// <see cref="GetCollectedItems"/>.
    /// </param>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected FaultyLoader(bool collectItems, IProgressTimer timer)
    {
        _collectItems  = collectItems;
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    // ------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns a snapshot of the items loaded so far, or <see langword="null"/> if the
    /// loader was constructed with <c>collectItems: false</c>.
    /// </summary>
    /// <returns>
    /// A <see cref="IReadOnlyList{T}"/> containing a point-in-time copy of the loaded items
    /// (including any injected duplicates), or <see langword="null"/> when collection is
    /// disabled.
    /// </returns>
    public IReadOnlyList<T>? GetCollectedItems() =>
        _collectItems
            ? _buffer.ToArray()
            : null;



    // ------------------------------------------------------------------
    // Fluent fault configuration
    // ------------------------------------------------------------------

    /// <summary>
    /// Configures the loader to throw <paramref name="exception"/> when it reaches the item
    /// at the specified zero-based <paramref name="index"/> in the loaded sequence. The
    /// failing item is counted (its
    /// <see cref="LoaderBase{TDestination,TProgress}.IncrementCurrentItemCount"/> runs) before
    /// the exception is thrown, so progress reflects the item that caused the failure, but
    /// the item itself is not stored.
    /// </summary>
    /// <param name="index">The zero-based index of the item to fail on.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The same <see cref="FaultyLoader{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var loader = new FaultyLoader&lt;int&gt;(collectItems: true)
    ///     .ThrowAt(25, new System.TimeoutException("connection lost"));
    /// </code>
    /// </example>
    public FaultyLoader<T> ThrowAt(int index, Exception exception)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _throwAt[index] = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the loader to throw <paramref name="exception"/> after all items have been
    /// loaded successfully, simulating a cleanup or finalization failure.
    /// </summary>
    /// <param name="exception">The exception to throw after completion.</param>
    /// <returns>The same <see cref="FaultyLoader{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var loader = new FaultyLoader&lt;int&gt;(collectItems: false)
    ///     .ThrowAfterCompletion(new System.InvalidOperationException("commit failed"));
    /// </code>
    /// </example>
    public FaultyLoader<T> ThrowAfterCompletion(Exception exception)
    {
        _throwAfterCompletion = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the loader to load the item at the specified zero-based
    /// <paramref name="index"/> twice. The duplicate is a real second load and is counted,
    /// so the total number of loaded items grows by one per configured duplicate.
    /// </summary>
    /// <param name="index">The zero-based index of the item to duplicate.</param>
    /// <returns>The same <see cref="FaultyLoader{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var loader = new FaultyLoader&lt;int&gt;(collectItems: true)
    ///     .DuplicateAt(10);
    /// </code>
    /// </example>
    public FaultyLoader<T> DuplicateAt(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _duplicateAt.Add(index);

        return this;
    }



    // ------------------------------------------------------------------
    // LoaderBase overrides
    // ------------------------------------------------------------------

    /// <inheritdoc/>
    protected override IProgressTimer CreateProgressTimer(IProgress<Report> progress)
    {
        if (_progressTimer is null)
        {
            return base.CreateProgressTimer(progress);
        }

        if (!_progressTimerWired)
        {
            _progressTimerWired = true;
            _progressTimer.Elapsed += () => progress.Report(CreateProgressReport());
        }

        return _progressTimer;
    }



    /// <inheritdoc/>
    protected override Report CreateProgressReport() =>
        new Report(CurrentItemCount);



    /// <inheritdoc/>
    protected override async Task LoadWorkerAsync(
        IAsyncEnumerable<T> items,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        _buffer.Clear();

        var index = 0;

        try
        {
            await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();

                if (CurrentSkippedItemCount < SkipItemCount)
                {
                    IncrementCurrentSkippedItemCount();
                    continue;
                }

                if (CurrentItemCount >= MaximumItemCount)
                {
                    break;
                }

                IncrementCurrentItemCount();

                if (_throwAt.TryGetValue(index, out var exception))
                {
                    throw exception;
                }

                if (_collectItems)
                {
                    _buffer.Add(item);
                }

                if (_duplicateAt.Contains(index) && CurrentItemCount < MaximumItemCount)
                {
                    IncrementCurrentItemCount();

                    if (_collectItems)
                    {
                        _buffer.Add(item);
                    }
                }

                index++;
            }
        }
        finally
        {
            _progressTimer?.StopTimer();
        }

        if (_throwAfterCompletion is not null)
        {
            throw _throwAfterCompletion;
        }
    }
}
