using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// An in-memory extractor for use in tests, examples, and benchmarks.
/// Yields items from either an <see cref="IEnumerable{T}"/> or an
/// <see cref="IEnumerator{T}"/>, depending on which constructor is used.
/// </summary>
/// <typeparam name="T">The type of item to extract.</typeparam>
/// <remarks>
/// <para>
/// Use the <see cref="IEnumerable{T}"/> constructor when your data is already
/// materialized in a collection. The enumerable is re-evaluated on each call to
/// <c>ExtractAsync</c>, so the extractor can be reused across multiple runs.
/// </para>
/// <para>
/// Use the <see cref="IEnumerator{T}"/> constructor when you need to generate large
/// volumes of data on the fly (e.g. via a generator method with <c>yield return</c>)
/// without materializing all items in memory at once. The caller owns the enumerator's
/// lifetime — the extractor does not dispose it.
/// </para>
/// <para>
/// Set <see cref="ExtractorBase{TSource,TProgress}.SkipItemCount"/> to skip the first N
/// items before yielding. Set
/// <see cref="ExtractorBase{TSource,TProgress}.MaximumItemCount"/> to stop after yielding
/// that many items. Both default to their base-class values (0 and
/// <see cref="int.MaxValue"/> respectively).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // From a list:
/// var items = new List&lt;int&gt; { 1, 2, 3, 4, 5 };
/// var extractor = new TestExtractor&lt;int&gt;(items);
///
/// // From an on-the-fly generator — avoids allocating a million-item list:
/// static IEnumerator&lt;int&gt; Generate(int count)
/// {
///     for (var i = 0; i &lt; count; i++)
///         yield return i;
/// }
///
/// var extractor = new TestExtractor&lt;int&gt;(Generate(1_000_000));
/// </code>
/// </example>
public class TestExtractor<T> : ExtractorBase<T, Report>
    where T : notnull
{
    // ------------------------------------------------------------------
    // Fields
    // ------------------------------------------------------------------

    private readonly IEnumerable<T>? _enumerable;
    private readonly IEnumerator<T>? _enumerator;
    private readonly IProgressTimer? _progressTimer;



    // ------------------------------------------------------------------
    // Constructors
    // ------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items from
    /// the specified <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="items">
    /// The sequence of items to extract. The enumerable is evaluated lazily on
    /// each extraction run, so the same extractor instance can be reused.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is <see langword="null"/>.
    /// </exception>
    public TestExtractor(IEnumerable<T> items)
    {
        _enumerable = items ?? throw new ArgumentNullException(nameof(items));
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items from
    /// the specified <see cref="IEnumerator{T}"/>.
    /// </summary>
    /// <param name="enumerator">
    /// The enumerator to draw items from. Useful for generator methods that produce
    /// large volumes of data on demand without allocating a full collection in memory.
    /// The caller is responsible for the enumerator's lifetime — the extractor does
    /// not dispose it.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="enumerator"/> is <see langword="null"/>.
    /// </exception>
    public TestExtractor(IEnumerator<T> enumerator)
    {
        _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items from
    /// the specified <see cref="IEnumerable{T}"/> and uses the supplied
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
    protected TestExtractor(IEnumerable<T> items, IProgressTimer timer)
    {
        _enumerable    = items ?? throw new ArgumentNullException(nameof(items));
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }



    /// <summary>
    /// Initializes a new <see cref="TestExtractor{T}"/> that yields items from
    /// the specified <see cref="IEnumerator{T}"/> and uses the supplied
    /// <see cref="IProgressTimer"/> to drive progress callbacks.
    /// </summary>
    /// <param name="enumerator">
    /// The enumerator to draw items from. The caller is responsible for the
    /// enumerator's lifetime — the extractor does not dispose it.
    /// </param>
    /// <param name="timer">
    /// The timer used to drive progress callbacks. Inject a
    /// <c>ManualProgressTimer</c> in tests to fire callbacks on demand.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="enumerator"/> or <paramref name="timer"/> is <see langword="null"/>.
    /// </exception>
    protected TestExtractor(IEnumerator<T> enumerator, IProgressTimer timer)
    {
        _enumerator    = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
        _progressTimer = timer      ?? throw new ArgumentNullException(nameof(timer));
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

        _progressTimer.Elapsed += () => progress.Report(CreateProgressReport());
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

        var enumerator     = (_enumerator ?? _enumerable!.GetEnumerator())!;
        var ownsEnumerator = _enumerator == null;

        try
        {
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

                IncrementCurrentItemCount();
                yield return enumerator.Current;
            }
        }
        finally
        {
            _progressTimer?.StopTimer();
            if (ownsEnumerator)
            {
                enumerator.Dispose();
            }
        }

        await Task.Yield(); // satisfies async method contract without causing extra allocations
    }
}
