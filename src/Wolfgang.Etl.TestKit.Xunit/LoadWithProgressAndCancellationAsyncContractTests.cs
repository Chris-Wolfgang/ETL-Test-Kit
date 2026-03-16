using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that implements
/// <see cref="ILoadWithProgressAndCancellationAsync{TDestination, TProgress}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement
/// <see cref="ILoadWithProgressAndCancellationAsync{TDestination, TProgress}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the loader consumes.</typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <remarks>
/// Inherit from this class to verify that your loader satisfies the full
/// <see cref="ILoadWithProgressAndCancellationAsync{TDestination, TProgress}"/>
/// contract — covering all four loader interfaces in the hierarchy.
/// </remarks>
/// <example>
/// <code>
/// public class MyLoaderFullContractTests
///     : LoadWithProgressAndCancellationAsyncContractTests&lt;MyLoader, MyRecord, MyProgress&gt;
/// {
///     protected override MyLoader CreateSut() => new MyLoader();
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateSourceItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c") };
/// }
/// </code>
/// </example>
public abstract class LoadWithProgressAndCancellationAsyncContractTests<TSut, TItem, TProgress>
    where TSut : ILoadWithProgressAndCancellationAsync<TItem, TProgress>
    where TItem : notnull
    where TProgress : notnull
{
    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>Creates the system under test.</summary>
    /// <returns>A new, fully initialised instance of <typeparamref name="TSut"/>.</returns>
    /// <summary>
    /// Creates the system under test configured to yield exactly <paramref name="itemCount"/> items.
    /// </summary>
    /// <param name="itemCount">The number of items the SUT should yield. Pass 0 for an empty source.</param>
    protected abstract TSut CreateSut(int itemCount);

    private const int DefaultItemCount = 5;

    /// <summary>Creates the SUT with <see cref="DefaultItemCount"/> items.</summary>
    private TSut CreateSut() => CreateSut(DefaultItemCount);


    /// <summary>
    /// Returns the source items used to feed the loader. Must return at least 5 items.
    /// </summary>
    protected abstract IReadOnlyList<TItem> CreateSourceItems();




    // ------------------------------------------------------------------
    // Tests — ILoadAsync<TItem>
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;)</c> completes without
    /// throwing when supplied a valid sequence.
    /// </summary>
    [Fact]
    public Task LoadAsync_completes_without_throwing()
    {
        var sut = CreateSut();

        return sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable());
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;)</c> completes without
    /// throwing when supplied an empty sequence.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_empty_sequence_completes_without_throwing()
    {
        var sut = CreateSut();

        return sut.LoadAsync(AsyncEnumerable.Empty<TItem>());
    }



    // ------------------------------------------------------------------
    // Tests — ILoadWithCancellationAsync<TItem>
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, CancellationToken)</c>
    /// completes without throwing when passed <see cref="CancellationToken.None"/>.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_cancellation_token_completes_without_throwing()
    {
        var sut = CreateSut();

        return sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable(), CancellationToken.None);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, CancellationToken)</c>
    /// throws <see cref="OperationCanceledException"/> when the token is cancelled
    /// mid-sequence.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_cancellation_token_stops_when_token_is_cancelled()
    {
        var sut = CreateSut();
        var source = CreateSourceItems();
        Assert.True(source.Count >= 3, "CreateSourceItems() must return at least 3 items.");

        using var cts = new CancellationTokenSource();
        var itemsLoaded = 0;

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
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



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, CancellationToken)</c>
    /// throws <see cref="OperationCanceledException"/> immediately when passed an
    /// already-cancelled token.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_already_cancelled_token_throws_OperationCanceledException()
    {
        var sut = CreateSut();
        using var cts = new CancellationTokenSource();
#if NET8_0_OR_GREATER
        await cts.CancelAsync();
#else
        cts.Cancel();
#endif

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable(), cts.Token));
    }



    // ------------------------------------------------------------------
    // Tests — ILoadWithProgressAsync<TItem, TProgress>
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// throws <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_null_progress_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        return Assert.ThrowsAsync<ArgumentNullException>(() => sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), (IProgress<TProgress>)null!));
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// completes without throwing when supplied valid arguments.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_progress_completes_without_throwing()
    {
        var sut = CreateSut();
        var progress = new Progress<TProgress>();

        return sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable(), progress);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// invokes the progress callback at least once during loading.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_invokes_callback_at_least_once()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable(), progress);

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
    // Tests — ILoadWithProgressAndCancellationAsync<TItem, TProgress>
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// throws <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_null_progress_and_token_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        return Assert.ThrowsAsync<ArgumentNullException>(() => sut.LoadAsync(AsyncEnumerable.Empty<TItem>(), (IProgress<TProgress>)null!, CancellationToken.None));
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// completes without throwing when supplied valid arguments.
    /// </summary>
    [Fact]
    public Task LoadAsync_with_progress_and_token_completes_without_throwing()
    {
        var sut = CreateSut();
        var progress = new Progress<TProgress>();

        return sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable(), progress, CancellationToken.None);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// invokes the progress callback at least once during loading.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_token_invokes_callback_at_least_once()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable(), progress, CancellationToken.None);

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;, CancellationToken)</c>
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



    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;, CancellationToken)</c>
    /// throws <see cref="OperationCanceledException"/> and stops loading when the token
    /// is cancelled mid-sequence.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_progress_and_cancelled_token_stops_loading()
    {
        var sut = CreateSut();
        var source = CreateSourceItems();
        Assert.True(source.Count >= 3, "CreateSourceItems() must return at least 3 items.");

        using var cts = new CancellationTokenSource();
        var itemsLoaded = 0;
        var progress = new SynchronousProgress<TProgress>(_ => { });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await sut.LoadAsync(CancellingSequenceAsync(), progress, cts.Token);

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
}
