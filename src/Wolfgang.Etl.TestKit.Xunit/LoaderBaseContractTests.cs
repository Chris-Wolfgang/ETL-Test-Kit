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
/// from <see cref="LoaderBase{TDestination, TProgress}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must inherit from
/// <see cref="LoaderBase{TItem, TProgress}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the loader consumes.</typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <remarks>
/// <para>
/// Inherit from this class to get a comprehensive suite of contract tests covering all
/// public behaviour defined by <see cref="LoaderBase{TDestination, TProgress}"/>:
/// </para>
/// <list type="bullet">
///   <item><description>All four <c>LoadAsync</c> overloads complete successfully.</description></item>
///   <item><description>Null guards on <c>items</c> and <c>progress</c> arguments.</description></item>
///   <item><description>Cancellation is honoured on all cancellable overloads.</description></item>
///   <item><description><c>CurrentItemCount</c> is incremented as items are loaded.</description></item>
///   <item><description><c>ReportingInterval</c> rejects values less than 1.</description></item>
///   <item><description><c>MaximumItemCount</c> stops loading at the specified limit.</description></item>
///   <item><description><c>MaximumItemCount</c> rejects values less than 0.</description></item>
///   <item><description><c>SkipItemCount</c> skips the specified number of items.</description></item>
///   <item><description><c>SkipItemCount</c> rejects values less than 0.</description></item>
///   <item><description>Progress callbacks fire when the timer fires.</description></item>
/// </list>
/// <para>
/// You are responsible for implementing <c>MaximumItemCount</c> and
/// <c>SkipItemCount</c> behaviour in your <c>LoadWorkerAsync</c> override.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyLoaderContractTests
///     : LoaderBaseContractTests&lt;MyLoader, MyRecord, MyProgress&gt;
/// {
///     protected override MyLoader CreateSut() => new MyLoader();
///
///     protected override MyLoader CreateSutWithTimer(IProgressTimer timer) =>
///         new MyLoader(timer);
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c"), new("d"), new("e") };
/// }
/// </code>
/// </example>
public abstract class LoaderBaseContractTests<TSut, TItem, TProgress>
    where TSut : LoaderBase<TItem, TProgress>
    where TItem : notnull
    where TProgress : notnull
{
    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates the system under test configured to yield exactly <paramref name="itemCount"/> items.
    /// </summary>
    /// <param name="itemCount">The number of items the SUT should yield. Pass 0 for an empty source.</param>
    protected abstract TSut CreateSut(int itemCount);

    /// <summary>
    /// Creates a <typeparamref name="TSut"/> with the supplied <see cref="IProgressTimer"/>
    /// injected via the derived class's protected constructor.
    /// </summary>
    /// <param name="timer">
    /// The <see cref="IProgressTimer"/> to inject. Typically a
    /// <see cref="ManualProgressTimer"/> so that progress callbacks can be fired
    /// on demand during tests.
    /// </param>
    protected abstract TSut CreateSutWithTimer(IProgressTimer timer);


    private const int DefaultItemCount = 5;

    /// <summary>Creates the SUT with <see cref="DefaultItemCount"/> items.</summary>
    private TSut CreateSut() => CreateSut(DefaultItemCount);

    /// <summary>Creates the SUT with an empty source.</summary>
    private TSut CreateSutWithNoItems() => CreateSut(0);

    /// <summary>Returns the default source items — integers 1 to <see cref="DefaultItemCount"/>.</summary>
    private IReadOnlyList<TItem> CreateSourceItems() =>
        Enumerable.Range(1, DefaultItemCount).Cast<TItem>().ToList();

    /// <summary>Returns the default input sequence — integers 1 to <see cref="DefaultItemCount"/>.</summary>
    private IAsyncEnumerable<TItem> CreateInputItems() =>
        Enumerable.Range(1, DefaultItemCount).Cast<TItem>().ToAsyncEnumerable();



    // ------------------------------------------------------------------
    // LoadAsync(IAsyncEnumerable) — basic load
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(items)</c> throws <see cref="ArgumentNullException"/>
    /// when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_null_items_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        return Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync((IAsyncEnumerable<TItem>)null!));
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items)</c> completes without error when valid
    /// items are supplied.
    /// </summary>
    [Fact]
    public Task LoadAsync_completes_successfully()
    {
        var sut = CreateSut();

        return sut.LoadAsync(CreateInputItems());
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items)</c> completes without error when the source
    /// contains no items.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_empty_source_completes_without_error()
    {
        var sut = CreateSutWithNoItems();

        return sut.LoadAsync(AsyncEnumerable.Empty<TItem>());
    }



    /// <summary>
    /// Verifies that <c>CurrentItemCount</c> equals the number of items loaded
    /// after a full load via <c>LoadAsync(items)</c>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_increments_CurrentItemCount_for_each_item()
    {
        var sut = CreateSut();
        var expected = CreateSourceItems();

        await sut.LoadAsync(CreateInputItems());

        Assert.Equal(expected.Count, sut.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // LoadAsync(IAsyncEnumerable, CancellationToken) — cancellation
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(items, CancellationToken)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_token_and_null_items_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        return Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync((IAsyncEnumerable<TItem>)null!, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, CancellationToken)</c> completes when passed
    /// <see cref="CancellationToken.None"/>.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_token_completes_successfully()
    {
        var sut = CreateSut();

        return sut.LoadAsync(CreateInputItems(), CancellationToken.None);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, CancellationToken)</c> completes without error
    /// when the source contains no items.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_token_and_empty_source_completes_without_error()
    {
        var sut = CreateSutWithNoItems();

        return sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), CancellationToken.None);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> when the token is already cancelled.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_cancelled_token_throws_OperationCanceledException()
    {
        var sut = CreateSut();
        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync();
#else
        cts.Cancel();
#endif

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.LoadAsync(CreateInputItems(), cts.Token));
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> and stops loading when the token is
    /// cancelled mid-sequence.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_cancellation_token_stops_when_token_is_cancelled()
    {
        var sut = CreateSut();
        var source = CreateSourceItems();
        Assert.True(source.Count >= 3, "CreateSourceItems() must return at least 3 items.");

        using var cts = new CancellationTokenSource();
        var itemsLoaded = 0;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await sut.LoadAsync(CancellingSequenceAsync(), cts.Token);

            async IAsyncEnumerable<TItem> CancellingSequenceAsync()
            {
                foreach (var item in source)
                {
                    itemsLoaded++;
                    if (itemsLoaded == 1)
                    {
#if NET8_0_OR_GREATER
                        await cts.CancelAsync();
#else
                        cts.Cancel();
#endif
                    }
                    yield return item;
                }
            }
        });

        Assert.Equal(1, itemsLoaded);
    }



    // ------------------------------------------------------------------
    // LoadAsync(IAsyncEnumerable, IProgress) — progress only
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_progress_and_null_items_throws_ArgumentNullException()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        return Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync((IAsyncEnumerable<TItem>)null!, progress));
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_null_progress_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        return Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync(CreateInputItems(), (IProgress<TProgress>)null!));
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress)</c> completes when valid arguments
    /// are supplied.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_progress_completes_successfully()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        return sut.LoadAsync(CreateInputItems(), progress);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress)</c> completes without error when
    /// the source contains no items.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_progress_and_empty_source_completes_without_error()
    {
        var sut = CreateSutWithNoItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        return sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), progress);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress)</c> invokes the progress callback
    /// when the <see cref="IProgressTimer"/> fires.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_invokes_callback_when_timer_fires()
    {
        using var timer = new ManualProgressTimer();
        var sut = CreateSutWithTimer(timer);
        TProgress? captured = default;
        var progress = new SynchronousProgress<TProgress>(r => captured = r);

        var task = sut.LoadAsync(CreateInputItems(), progress);
        timer.Fire();
        await task;

        Assert.NotNull(captured);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, IProgress&lt;TProgress&gt;)</c> invokes the progress
    /// callback at least once during loading when using a standard <see cref="Progress{T}"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_invokes_callback_at_least_once()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.LoadAsync(CreateInputItems(), progress);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, IProgress&lt;TProgress&gt;)</c> invokes the
    /// progress callback at least once even when the source is empty.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_empty_source_invokes_callback_at_least_once()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), progress);

        Assert.True(callbackCount >= 1);
    }



    // ------------------------------------------------------------------
    // LoadAsync(IAsyncEnumerable, IProgress, CancellationToken)
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>items</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_progress_and_token_and_null_items_throws_ArgumentNullException()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        return Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync((IAsyncEnumerable<TItem>)null!, progress, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> throws
    /// <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_progress_and_token_and_null_progress_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        return Assert.ThrowsAsync<ArgumentNullException>(() =>
            sut.LoadAsync(CreateInputItems(), (IProgress<TProgress>)null!, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> completes
    /// when valid arguments are supplied.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_progress_and_token_completes_successfully()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        return sut.LoadAsync(CreateInputItems(), progress, CancellationToken.None);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> completes
    /// without error when the source contains no items.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_progress_and_token_and_empty_source_completes_without_error()
    {
        var sut = CreateSutWithNoItems();
        var progress = new SynchronousProgress<TProgress>(_ => { });

        return sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), progress, CancellationToken.None);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> throws
    /// <see cref="OperationCanceledException"/> when the token is already cancelled.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_cancelled_token_throws_OperationCanceledException()
    {
        var sut = CreateSut();
        var progress = new SynchronousProgress<TProgress>(_ => { });
        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync();
#else
        cts.Cancel();
#endif

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.LoadAsync(CreateInputItems(), progress, cts.Token));
    }

    /// <summary>
    /// Verifies that <c>LoadAsync(items, progress, CancellationToken)</c> invokes the
    /// progress callback when the <see cref="IProgressTimer"/> fires.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_token_invokes_callback_when_timer_fires()
    {
        using var timer = new ManualProgressTimer();
        var sut = CreateSutWithTimer(timer);
        TProgress? captured = default;
        var progress = new SynchronousProgress<TProgress>(r => captured = r);

        var task = sut.LoadAsync(CreateInputItems(), progress, CancellationToken.None);
        timer.Fire();
        await task;

        Assert.NotNull(captured);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once during loading when using a standard
    /// <see cref="Progress{T}"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_token_invokes_callback_at_least_once()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.LoadAsync(CreateInputItems(), progress, CancellationToken.None);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(items, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once even when the source is empty.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_token_and_empty_source_invokes_callback_at_least_once()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), progress, CancellationToken.None);

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
    /// <see cref="ArgumentOutOfRangeException"/> — the minimum for a loader is 1.
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
    /// Verifies that the loader stops consuming items once <c>MaximumItemCount</c>
    /// is reached.
    /// </summary>
    [Fact]
    public async Task LoadAsync_stops_at_MaximumItemCount()
    {
        var sut = CreateSut();
        var expected = CreateSourceItems();
        Assert.True(expected.Count >= 3, "CreateSourceItems() must return at least 3 items.");

        sut.MaximumItemCount = 1;

        await sut.LoadAsync(CreateInputItems());

        Assert.Equal(1, sut.CurrentItemCount);
    }

    /// <summary>
    /// Verifies that when <c>MaximumItemCount</c> exceeds the sequence length, all items
    /// are still loaded.
    /// </summary>
    [Fact]
    public async Task LoadAsync_loads_all_items_when_MaximumItemCount_exceeds_sequence_length()
    {
        var sut = CreateSut();
        var expected = CreateSourceItems();
        sut.MaximumItemCount = expected.Count + 100;

        await sut.LoadAsync(CreateInputItems());

        Assert.Equal(expected.Count, sut.CurrentItemCount);
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
    /// Verifies that the loader skips the specified number of items before loading.
    /// </summary>
    [Fact]
    public async Task LoadAsync_skips_items_up_to_SkipItemCount()
    {
        var sut = CreateSut();
        var expected = CreateSourceItems();
        Assert.True(expected.Count >= 3, "CreateSourceItems() must return at least 3 items.");

        sut.SkipItemCount = 1;

        await sut.LoadAsync(CreateInputItems());

        Assert.Equal(expected.Count - 1, sut.CurrentItemCount);
        Assert.Equal(1, sut.CurrentSkippedItemCount);
    }
}
