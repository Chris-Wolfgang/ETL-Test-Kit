using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// An in-memory extractor for testing error-handling paths. It yields items from an
/// <see cref="IEnumerable{T}"/> like a normal extractor, but can be configured to inject
/// deterministic faults — throwing at a given item, throwing after completion, or
/// duplicating an item — so consumers can exercise mid-stream failure, finalization
/// failure, and de-duplication logic without hand-rolling broken fakes.
/// </summary>
/// <typeparam name="T">The type of item to extract.</typeparam>
/// <remarks>
/// <para>
/// Faults are configured through the fluent <see cref="ThrowAt"/>,
/// <see cref="ThrowAfterCompletion"/>, and <see cref="DuplicateAt"/> methods, each of
/// which returns the same instance so calls can be chained. Multiple faults stack on a
/// single instance — for example <c>ThrowAt(50, ex)</c> and <c>DuplicateAt(10)</c> both
/// take effect in the same run.
/// </para>
/// <para>
/// Fault indices are zero-based and refer to the position in the emitted (post-skip)
/// sequence. A configured fault fires <em>after</em>
/// <see cref="ExtractorBase{TSource,TProgress}.IncrementCurrentItemCount"/> for that item,
/// so a progress report reflects the item that caused the failure. When a
/// <see cref="ThrowAt"/> and a <see cref="DuplicateAt"/> are configured for the same index,
/// the throw takes precedence and the duplicate is not emitted. Calling <see cref="ThrowAt"/>
/// twice for the same index replaces the earlier exception (last-wins).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var items     = new List&lt;int&gt; { 1, 2, 3, 4, 5 };
/// var extractor = new FaultyExtractor&lt;int&gt;(items)
///     .ThrowAt(index: 3, new System.IO.IOException("disk read failure"))
///     .DuplicateAt(index: 1);
///
/// // Enumerates 1, 2, 2 (duplicate), 3, then throws IOException reaching index 3.
/// await foreach (var item in extractor.ExtractAsync()) { /* ... */ }
/// </code>
/// </example>
public class FaultyExtractor<T> : ExtractorBase<T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly IEnumerable<T> _items;
    private readonly Dictionary<int, Exception> _throwAt = new Dictionary<int, Exception>();
    private readonly HashSet<int> _duplicateAt = new HashSet<int>();
    private Exception? _throwAfterCompletion;
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;



    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="FaultyExtractor{T}"/> that yields items from the
    /// specified <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="items">
    /// The sequence of items to extract. The enumerable is evaluated on each extraction
    /// run, so the same extractor instance can be reused.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is <see langword="null"/>.
    /// </exception>
    public FaultyExtractor(IEnumerable<T> items)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
    }



    /// <summary>
    /// Initializes a new <see cref="FaultyExtractor{T}"/> that yields items from the
    /// specified <see cref="IEnumerable{T}"/> and uses the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks.
    /// </summary>
    /// <param name="items">The sequence of items to extract.</param>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> or <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected FaultyExtractor(IEnumerable<T> items, IProgressTimer timer)
    {
        _items         = items ?? throw new ArgumentNullException(nameof(items));
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    // ------------------------------------------------------------------
    // Fluent fault configuration
    // ------------------------------------------------------------------

    /// <summary>
    /// Configures the extractor to throw <paramref name="exception"/> when it reaches the
    /// item at the specified zero-based <paramref name="index"/> in the emitted sequence.
    /// The failing item is counted (its
    /// <see cref="ExtractorBase{TSource,TProgress}.IncrementCurrentItemCount"/> runs) before
    /// the exception is thrown, so progress reflects the item that caused the failure, but
    /// the item itself is not yielded.
    /// </summary>
    /// <param name="index">The zero-based index of the item to fail on.</param>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The same <see cref="FaultyExtractor{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var extractor = new FaultyExtractor&lt;int&gt;(items)
    ///     .ThrowAt(50, new System.IO.IOException("disk read failure"));
    /// </code>
    /// </example>
    public FaultyExtractor<T> ThrowAt(int index, Exception exception)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _throwAt[index] = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the extractor to throw <paramref name="exception"/> after all items have
    /// been yielded successfully, simulating a cleanup or finalization failure.
    /// </summary>
    /// <param name="exception">The exception to throw after completion.</param>
    /// <returns>The same <see cref="FaultyExtractor{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exception"/> is <see langword="null"/>.
    /// </exception>
    /// <example>
    /// <code>
    /// var extractor = new FaultyExtractor&lt;int&gt;(items)
    ///     .ThrowAfterCompletion(new System.InvalidOperationException("finalize failed"));
    /// </code>
    /// </example>
    public FaultyExtractor<T> ThrowAfterCompletion(Exception exception)
    {
        _throwAfterCompletion = exception ?? throw new ArgumentNullException(nameof(exception));

        return this;
    }



    /// <summary>
    /// Configures the extractor to yield the item at the specified zero-based
    /// <paramref name="index"/> twice. The duplicate is a real second emission and is
    /// counted, so the total number of yielded items grows by one per configured duplicate.
    /// </summary>
    /// <param name="index">The zero-based index of the item to duplicate.</param>
    /// <returns>The same <see cref="FaultyExtractor{T}"/> instance, to allow chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// var extractor = new FaultyExtractor&lt;int&gt;(items)
    ///     .DuplicateAt(10);
    /// </code>
    /// </example>
    public FaultyExtractor<T> DuplicateAt(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _duplicateAt.Add(index);

        return this;
    }



    // ------------------------------------------------------------------
    // ExtractorBase overrides
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
    protected override Report CreateProgressReport() => new(CurrentItemCount);



    /// <inheritdoc/>
    protected override async IAsyncEnumerable<T> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        token.ThrowIfCancellationRequested();

        // The wrapped source is synchronous, so this iterator would otherwise
        // contain no await. Yield once up front to honour the async-iterator
        // contract on every exit path (including the MaximumItemCount yield-break
        // and ThrowAfterCompletion throw), reachable however the loop terminates.
        await Task.Yield();

        var enumerator = _items.GetEnumerator();

        try
        {
            var index = 0;

            while (enumerator.MoveNext())
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

                var item = enumerator.Current;

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
            enumerator.Dispose();
        }

        if (_throwAfterCompletion is not null)
        {
            throw _throwAfterCompletion;
        }
    }
}
