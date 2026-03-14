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
/// <see cref="ILoadWithCancellationAsync{TDestination}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement <see cref="ILoadWithCancellationAsync{TDestination}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the loader consumes.</typeparam>
/// <remarks>
/// The source items sequence must contain at least three items so that cancellation
/// mid-sequence can be meaningfully tested.
/// </remarks>
/// <example>
/// <code>
/// public class MyLoaderCancellationContractTests
///     : LoadWithCancellationAsyncContractTests&lt;MyLoader, MyRecord&gt;
/// {
///     protected override MyLoader CreateSut() => new MyLoader();
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateSourceItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c") };
/// }
/// </code>
/// </example>
public abstract class LoadWithCancellationAsyncContractTests<TSut, TItem>
    where TSut : ILoadWithCancellationAsync<TItem>
    where TItem : notnull
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


    /// <summary>Returns the default source items — integers 1 to <see cref="DefaultItemCount"/>.</summary>
    private IReadOnlyList<TItem> CreateSourceItems() =>
        Enumerable.Range(1, DefaultItemCount).Cast<TItem>().ToList();




    // ------------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>LoadAsync(IAsyncEnumerable&lt;TItem&gt;, CancellationToken)</c>
    /// completes without throwing when passed <see cref="CancellationToken.None"/>.
    /// </summary>
    [Fact]
    public async Task LoadAsync_with_cancellation_token_completes_without_throwing()
    {
        var sut = CreateSut();

        await sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable(), CancellationToken.None);
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
            await sut.LoadAsync(
                source.ToAsyncEnumerable().Select(item =>
                {
                    itemsLoaded++;
                    if (itemsLoaded == 1) cts.Cancel();
                    return item;
                }),
                cts.Token);
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
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await sut.LoadAsync(CreateSourceItems().ToAsyncEnumerable(), cts.Token);
        });
    }
}
