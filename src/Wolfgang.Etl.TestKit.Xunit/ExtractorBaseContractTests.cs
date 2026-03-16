using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that inherits
/// from <see cref="ExtractorBase{TSource, TProgress}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must inherit from
/// <see cref="ExtractorBase{TItem, TProgress}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the extractor yields.</typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <remarks>
/// <para>
/// Inherit from this class to get a comprehensive suite of contract tests covering all
/// public behaviour defined by <see cref="ExtractorBase{TSource, TProgress}"/>:
/// </para>
/// <list type="bullet">
///   <item><description>All four <c>ExtractAsync</c> overloads yield the expected items.</description></item>
///   <item><description>Cancellation is honoured on all cancellable overloads.</description></item>
///   <item><description><c>CurrentItemCount</c> is incremented as items are yielded.</description></item>
///   <item><description><c>ReportingInterval</c> rejects values less than 1.</description></item>
///   <item><description><c>MaximumItemCount</c> stops extraction at the specified limit.</description></item>
///   <item><description><c>MaximumItemCount</c> rejects values less than 0.</description></item>
///   <item><description><c>SkipItemCount</c> rejects values less than 0.</description></item>
///   <item><description>Progress callbacks fire at least once per extraction.</description></item>
///   <item><description><c>CreateProgressReport</c> reflects <c>CurrentItemCount</c>.</description></item>
/// </list>
/// <para>
/// You are responsible for implementing <c>MaximumItemCount</c> behaviour in
/// your <c>ExtractWorkerAsync</c> override. The contract tests for
/// <see cref="ExtractorBase{TSource, TProgress}.MaximumItemCount"/> will only pass if
/// your extractor checks that property and stops yielding when the limit is reached.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyExtractorContractTests
///     : ExtractorBaseContractTests&lt;MyExtractor, MyRecord, MyProgress&gt;
/// {
///     protected override MyExtractor CreateSut(int itemCount) =>
///         new MyExtractor("path/to/test-data.csv", itemCount);
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c"), new("d"), new("e") };
///
///     protected override MyExtractor CreateSutWithTimer(IProgressTimer timer) =>
///         new MyExtractor("path/to/test-data.csv", timer);
/// }
/// </code>
/// </example>
public abstract class ExtractorBaseContractTests<TSut, TItem, TProgress>
    where TSut : ExtractorBase<TItem, TProgress>
    where TItem : notnull
    where TProgress : notnull
{
    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the system under test configured to yield exactly <paramref name="itemCount"/> items.
    /// The items yielded must match the first <paramref name="itemCount"/> items returned by
    /// <see cref="CreateExpectedItems"/>.
    /// </summary>
    /// <param name="itemCount">The number of items the SUT should yield. Pass 0 for an empty source.</param>
    protected abstract TSut CreateSut(int itemCount);

    private const int DefaultItemCount = 5;

    /// <summary>Creates the SUT with <see cref="DefaultItemCount"/> items.</summary>
    private TSut CreateSut() => CreateSut(DefaultItemCount);

    /// <summary>Creates the SUT with an empty source.</summary>
    private TSut CreateSutWithNoItems() => CreateSut(0);

    /// <summary>
    /// Returns the expected items that the SUT should yield when created with
    /// <see cref="CreateSut(int)"/>. Must return at least 5 items.
    /// The first <c>itemCount</c> items from this list are the expected output
    /// when <c>CreateSut(itemCount)</c> is called.
    /// </summary>
    protected abstract IReadOnlyList<TItem> CreateExpectedItems();

    /// <summary>
    /// Creates the system under test with the supplied <see cref="IProgressTimer"/>
    /// injected via the derived class's protected constructor.
    /// </summary>
    /// <param name="timer">
    /// The <see cref="IProgressTimer"/> to inject. Typically a
    /// <see cref="ManualProgressTimer"/> so that progress callbacks can be fired
    /// on demand during tests.
    /// </param>
    /// <returns>A new, fully initialised instance of <typeparamref name="TSut"/>.</returns>
    /// <example>
    /// <code>
    /// protected override MyExtractor CreateSutWithTimer(IProgressTimer timer) =>
    ///     new MyExtractor(sourceData, timer);
    /// </code>
    /// </example>
    protected abstract TSut CreateSutWithTimer(IProgressTimer timer);





    // ------------------------------------------------------------------
    // ExtractAsync() — basic extraction
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>ExtractAsync()</c> returns a non-null sequence.
    /// </summary>
    [Fact]
    public void ExtractAsync_returns_non_null_sequence()
    {
        var sut = CreateSut();
        var result = sut.ExtractAsync();
        Assert.NotNull(result);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync()</c> yields all expected items in order.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_yields_all_expected_items_in_order()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var actual = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync()</c> yields an empty sequence without error
    /// when the source contains no items.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_empty_source_yields_no_items()
    {
        var sut = CreateSutWithNoItems();

        var actual = await sut.ExtractAsync().ToListAsync();

        Assert.Empty(actual);
    }



    /// <summary>
    /// Verifies that <c>CurrentItemCount</c> equals the number of items yielded
    /// after a full extraction via <c>ExtractAsync()</c>.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_increments_CurrentItemCount_for_each_item()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        await sut.ExtractAsync().ToListAsync();

        Assert.Equal(expected.Count, sut.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // ExtractAsync(CancellationToken) — cancellation
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>ExtractAsync(CancellationToken)</c> yields all expected items
    /// when passed <see cref="CancellationToken.None"/>.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_token_yields_all_expected_items()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var actual = await sut.ExtractAsync(CancellationToken.None).ToListAsync();

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(CancellationToken)</c> yields an empty sequence
    /// without error when the source contains no items.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_token_and_empty_source_yields_no_items()
    {
        var sut = CreateSutWithNoItems();

        var actual = await sut.ExtractAsync(CancellationToken.None).ToListAsync();

        Assert.Empty(actual);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> and stops yielding when the token is cancelled.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_token_stops_when_token_is_cancelled()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        using var cts = new CancellationTokenSource();
        var received = new List<TItem>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in sut.ExtractAsync(cts.Token))
            {
                received.Add(item);
                if (received.Count == 1)
                {
                    #if NET8_0_OR_GREATER
                    await cts.CancelAsync();
                    #else
                    cts.Cancel();
                    #endif
                }
            }
        });

        Assert.Equal(1, received.Count);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> immediately when passed an already-cancelled token.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_already_cancelled_token_throws_OperationCanceledException()
    {
        var sut = CreateSut();
        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync();
#else
        cts.Cancel();
#endif

        var received = new List<TItem>();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in sut.ExtractAsync(cts.Token))
            {
                received.Add(item);
            }
        });

        Assert.Empty(received);
    }



    // ------------------------------------------------------------------
    // ExtractAsync(IProgress<TProgress>) — progress only
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void ExtractAsync_with_null_progress_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.ExtractAsync((IProgress<TProgress>)null!);
        });
    }

    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;)</c> yields all expected
    /// items when a valid progress instance is supplied.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_yields_all_expected_items()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var actual = await sut.ExtractAsync(progress).ToListAsync();

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;)</c> yields an empty
    /// sequence without error when the source contains no items.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_empty_source_yields_no_items()
    {
        var sut = CreateSutWithNoItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var actual = await sut.ExtractAsync(progress).ToListAsync();

        Assert.Empty(actual);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;)</c> invokes the progress
    /// callback when the <see cref="IProgressTimer"/> fires.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_invokes_callback_when_timer_fires()
    {
        using var timer = new ManualProgressTimer();
        var sut = CreateSutWithTimer(timer);
        TProgress? captured = default;
        var progress = new SynchronousProgress<TProgress>(r => captured = r);

        await using var enumerator = sut.ExtractAsync(progress).GetAsyncEnumerator();
        await enumerator.MoveNextAsync();
        timer.Fire();

        Assert.NotNull(captured);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;)</c> invokes the progress
    /// callback at least once during extraction when using a standard <see cref="Progress{T}"/>.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_invokes_callback_at_least_once()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.ExtractAsync(progress).ToListAsync();

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;)</c> invokes the progress
    /// callback at least once even when the source is empty.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_empty_source_invokes_callback_at_least_once()
    {
        var sut = CreateSutWithNoItems();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.ExtractAsync(progress).ToListAsync();

        Assert.True(callbackCount >= 1);
    }



    // ------------------------------------------------------------------
    // ExtractAsync(IProgress<TProgress>, CancellationToken)
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// throws <see cref="ArgumentNullException"/> when <c>progress</c> is
    /// <see langword="null"/>.
    /// </summary>
    [Fact]
    public void ExtractAsync_with_null_progress_and_token_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.ExtractAsync((IProgress<TProgress>)null!, CancellationToken.None);
        });
    }

    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// yields all expected items when supplied valid arguments.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_token_yields_all_expected_items()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var actual = await sut.ExtractAsync(progress, CancellationToken.None).ToListAsync();

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// yields an empty sequence without error when the source contains no items.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_token_and_empty_source_yields_no_items()
    {
        var sut = CreateSutWithNoItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        var actual = await sut.ExtractAsync(progress, CancellationToken.None).ToListAsync();

        Assert.Empty(actual);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// throws <see cref="OperationCanceledException"/> and stops yielding when the token
    /// is cancelled mid-sequence.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_cancelled_token_stops_enumeration()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        using var cts = new CancellationTokenSource();
        var received = new List<TItem>();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in sut.ExtractAsync(progress, cts.Token))
            {
                received.Add(item);
                if (received.Count == 1)
                {
                    #if NET8_0_OR_GREATER
                    await cts.CancelAsync();
                    #else
                    cts.Cancel();
                    #endif
                }
            }
        });

        Assert.Equal(1, received.Count);
    }

    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback when the <see cref="IProgressTimer"/> fires.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_token_invokes_callback_when_timer_fires()
    {
        using var timer = new ManualProgressTimer();
        var sut = CreateSutWithTimer(timer);
        TProgress? captured = default;
        var progress = new SynchronousProgress<TProgress>(r => captured = r);

        await using var enumerator = sut.ExtractAsync(progress, CancellationToken.None).GetAsyncEnumerator();
        await enumerator.MoveNextAsync();
        timer.Fire();

        Assert.NotNull(captured);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once during extraction when using a standard
    /// <see cref="Progress{T}"/>.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_token_invokes_callback_at_least_once()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.ExtractAsync(progress, CancellationToken.None).ToListAsync();

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>ExtractAsync(IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once even when the source is empty.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_with_progress_and_token_and_empty_source_invokes_callback_at_least_once()
    {
        var sut = CreateSutWithNoItems();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.ExtractAsync(progress, CancellationToken.None).ToListAsync();

        Assert.True(callbackCount >= 1);
    }



    // ------------------------------------------------------------------
    // ReportingInterval
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>ReportingInterval</c> defaults to 1000.
    /// </summary>
    [Fact]
    public void ReportingInterval_defaults_to_1000()
    {
        var sut = CreateSut();
        Assert.Equal(1_000, sut.ReportingInterval);
    }

    /// <summary>
    /// Verifies that setting <c>ReportingInterval</c> to a positive value succeeds.
    /// </summary>
    [Fact]
    public void ReportingInterval_can_be_set_to_positive_value()
    {
        var sut = CreateSut();
        sut.ReportingInterval = 1;
        Assert.Equal(1, sut.ReportingInterval);
    }

    /// <summary>
    /// Verifies that setting <c>ReportingInterval</c> to zero throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void ReportingInterval_set_to_zero_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = 0);
    }

    /// <summary>
    /// Verifies that setting <c>ReportingInterval</c> to a negative value throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void ReportingInterval_set_to_negative_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReportingInterval = -1);
    }



    // ------------------------------------------------------------------
    // MaximumItemCount
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>MaximumItemCount</c> defaults to <see cref="int.MaxValue"/>.
    /// </summary>
    [Fact]
    public void MaximumItemCount_defaults_to_int_MaxValue()
    {
        var sut = CreateSut();
        Assert.Equal(int.MaxValue, sut.MaximumItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>MaximumItemCount</c> to a positive value succeeds.
    /// </summary>
    [Fact]
    public void MaximumItemCount_can_be_set_to_positive_value()
    {
        var sut = CreateSut();
        sut.MaximumItemCount = 10;
        Assert.Equal(10, sut.MaximumItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>MaximumItemCount</c> to zero throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void MaximumItemCount_set_to_zero_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = 0);
    }

    /// <summary>
    /// Verifies that setting <c>MaximumItemCount</c> to a negative value throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void MaximumItemCount_set_to_negative_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.MaximumItemCount = -1);
    }

    /// <summary>
    /// Verifies that the extractor stops yielding items once <c>MaximumItemCount</c>
    /// is reached, returning fewer items than the full sequence.
    /// </summary>
    /// <remarks>
    /// This test only passes if your <c>ExtractWorkerAsync</c> implementation checks
    /// <see cref="ExtractorBase{TSource, TProgress}.MaximumItemCount"/> and stops
    /// yielding when the limit is reached.
    /// </remarks>
    [Fact]
    public async Task ExtractAsync_stops_at_MaximumItemCount()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        sut.MaximumItemCount = 1;

        var actual = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(1, actual.Count);
        Assert.Equal(expected[0], actual[0]);
    }

    /// <summary>
    /// Verifies that when <c>MaximumItemCount</c> is set to a value equal to or greater
    /// than the total number of items, all items are still yielded.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_yields_all_items_when_MaximumItemCount_exceeds_sequence_length()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        sut.MaximumItemCount = expected.Count + 100;

        var actual = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(expected, actual);
    }



    // ------------------------------------------------------------------
    // SkipItemCount
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>SkipItemCount</c> defaults to zero.
    /// </summary>
    [Fact]
    public void SkipItemCount_defaults_to_zero()
    {
        var sut = CreateSut();
        Assert.Equal(0, sut.SkipItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>SkipItemCount</c> to a positive value succeeds.
    /// </summary>
    [Fact]
    public void SkipItemCount_can_be_set_to_positive_value()
    {
        var sut = CreateSut();
        sut.SkipItemCount = 5;
        Assert.Equal(5, sut.SkipItemCount);
    }

    /// <summary>
    /// Verifies that setting <c>SkipItemCount</c> to a negative value throws
    /// <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void SkipItemCount_set_to_negative_throws_ArgumentOutOfRangeException()
    {
        var sut = CreateSut();
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SkipItemCount = -1);
    }

    /// <summary>
    /// Verifies that <c>SkipItemCount</c> causes the extractor to skip the first
    /// N items and only yield items after the skip budget is exhausted.
    /// </summary>
    [Fact]
    public async Task ExtractAsync_skips_items_up_to_SkipItemCount()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        Assert.True(expected.Count >= 3, "CreateExpectedItems() must return at least 3 items.");

        sut.SkipItemCount = 1;

        var actual = await sut.ExtractAsync().ToListAsync();

        Assert.Equal(expected.Count - 1, actual.Count);
        Assert.Equal(expected[1], actual[0]);
        Assert.Equal(1, sut.CurrentSkippedItemCount);
    }
}
