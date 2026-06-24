using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// A pass-through transformer for testing error-handling paths. It returns each item
/// unchanged like a normal transformer, but can be configured to inject deterministic
/// faults — throwing at a given item, throwing after completion, or duplicating an item —
/// so consumers can exercise mid-stream failure, finalization failure, and de-duplication
/// logic without hand-rolling broken fakes.
/// </summary>
/// <typeparam name="T">The type of item to transform.</typeparam>
/// <remarks>
/// <para>
/// Faults are configured through the fluent <see cref="ThrowAt"/>,
/// <see cref="ThrowAfterCompletion"/>, and <see cref="DuplicateAt"/> methods, each of which
/// returns the same instance so calls can be chained. Multiple faults stack on a single
/// instance.
/// </para>
/// <para>
/// Fault indices are zero-based and refer to the position in the emitted (post-skip)
/// sequence. A configured fault fires <em>after</em>
/// <see cref="TransformerBase{TSource,TDestination,TProgress}.IncrementCurrentItemCount"/>
/// for that item, so a progress report reflects the item that caused the failure. When a
/// <see cref="ThrowAt"/> and a <see cref="DuplicateAt"/> are configured for the same index,
/// the throw takes precedence and the duplicate is not emitted. Calling <see cref="ThrowAt"/>
/// twice for the same index replaces the earlier exception (last-wins).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var transformer = new FaultyTransformer&lt;int&gt;()
///     .ThrowAt(index: 50, new System.InvalidOperationException("bad record"))
///     .DuplicateAt(index: 10);
///
/// await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));
/// </code>
/// </example>
public class FaultyTransformer<T> : TransformerBase<T, T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly Dictionary<int, Exception> _throwAt = new Dictionary<int, Exception>();
    private readonly HashSet<int> _duplicateAt = new HashSet<int>();
    private Exception? _throwAfterCompletion;
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;



    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="FaultyTransformer{T}"/> using the default production
    /// timer.
    /// </summary>
    public FaultyTransformer() { }



    /// <summary>
    /// Initializes a new <see cref="FaultyTransformer{T}"/> with the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks.
    /// </summary>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected FaultyTransformer(IProgressTimer timer)
    {
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    // ------------------------------------------------------------------
    // Fluent fault configuration
    // ------------------------------------------------------------------

    /// <summary>
    /// Configures the transformer to throw <paramref name="exception"/> when it reaches the
    /// item at the specified zero-based <paramref name="index"/> in the emitted sequence.
    /// The failing item is counted (its
    /// <see cref="TransformerBase{TSource,TDestination,TProgress}.IncrementCurrentItemCount"/>
    /// runs) before the exception is thrown, so progress reflects the item that caused the
    /// failure, but the item itself is not emitted.
    /// </summary>
    /// <param name="index">The zero-based index of the item to fail on.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The same <see cref="FaultyTransformer{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var transformer = new FaultyTransformer&lt;int&gt;()
    ///     .ThrowAt(50, new System.InvalidOperationException("bad record"));
    /// </code>
    /// </example>
    public FaultyTransformer<T> ThrowAt(int index, Exception exception)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _throwAt[index] = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the transformer to throw <paramref name="exception"/> after all items have
    /// been emitted successfully, simulating a cleanup or finalization failure.
    /// </summary>
    /// <param name="exception">The exception to throw after completion.</param>
    /// <returns>The same <see cref="FaultyTransformer{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var transformer = new FaultyTransformer&lt;int&gt;()
    ///     .ThrowAfterCompletion(new System.InvalidOperationException("flush failed"));
    /// </code>
    /// </example>
    public FaultyTransformer<T> ThrowAfterCompletion(Exception exception)
    {
        _throwAfterCompletion = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the transformer to emit the item at the specified zero-based
    /// <paramref name="index"/> twice. The duplicate is a real second emission and is
    /// counted, so the total number of emitted items grows by one per configured duplicate.
    /// </summary>
    /// <param name="index">The zero-based index of the item to duplicate.</param>
    /// <returns>The same <see cref="FaultyTransformer{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var transformer = new FaultyTransformer&lt;int&gt;()
    ///     .DuplicateAt(10);
    /// </code>
    /// </example>
    public FaultyTransformer<T> DuplicateAt(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _duplicateAt.Add(index);

        return this;
    }



    // ------------------------------------------------------------------
    // TransformerBase overrides
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
    protected override async IAsyncEnumerable<T> TransformWorkerAsync(
        IAsyncEnumerable<T> items,
        [EnumeratorCancellation] CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

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
                    yield break;
                }

                IncrementCurrentItemCount();

                if (_throwAt.TryGetValue(index, out var exception))
                {
                    throw exception;
                }

                yield return item;

                if (_duplicateAt.Contains(index) && CurrentItemCount < MaximumItemCount)
                {
                    IncrementCurrentItemCount();
                    yield return item;
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
