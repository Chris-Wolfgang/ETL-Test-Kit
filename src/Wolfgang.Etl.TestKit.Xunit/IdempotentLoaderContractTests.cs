using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing opt-in xUnit idempotency contract tests for any type that
/// implements <see cref="ILoadAsync{TDestination}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement <see cref="ILoadAsync{TDestination}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the loader consumes.</typeparam>
/// <remarks>
/// <para>
/// <b>This base class is opt-in.</b> Inherit from it <em>only</em> if your loader supports
/// being run more than once on the <em>same</em> instance, processing the full input each time
/// and carrying no state over from a previous run. Single-use components — for example a loader
/// that opens a destination stream once and disposes it after the first load — must <em>not</em>
/// inherit this class.
/// </para>
/// <para>
/// Each test calls <see cref="CreateSut(int)"/> exactly once and then loads
/// <see cref="CreateSourceItems"/> twice, asserting that the second run is unaffected by the
/// first.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyLoaderIdempotencyTests
///     : IdempotentLoaderContractTests&lt;MyLoader, MyRecord&gt;
/// {
///     protected override MyLoader CreateSut(int itemCount) =>
///         new MyLoader(collectItems: true);
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateSourceItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c"), new("d"), new("e") };
///
///     protected override IReadOnlyList&lt;MyRecord&gt;? TryGetLoadedItems(MyLoader sut) =>
///         sut.GetCollectedItems();
/// }
/// </code>
/// </example>
public abstract class IdempotentLoaderContractTests<TSut, TItem>
    where TSut : ILoadAsync<TItem>
    where TItem : notnull
{
    // NOTE: A companion test asserting that CurrentItemCount resets to zero between runs is
    // intentionally deferred. Today CurrentItemCount is cumulative across runs on the same
    // instance; whether it should reset is under decision in ETL-Abstractions#246. For the
    // same reason these tests rely on the default MaximumItemCount — setting it would cause a
    // second run to immediately hit the cumulative limit and load nothing.

    // ------------------------------------------------------------------
    // Factory methods
    // ------------------------------------------------------------------

    /// <summary>Creates the system under test.</summary>
    /// <param name="itemCount">
    /// The number of items the SUT is sized for. Pass 0 for an empty source.
    /// </param>
    protected abstract TSut CreateSut(int itemCount);

    private const int DefaultItemCount = 5;

    /// <summary>Creates the SUT with <see cref="DefaultItemCount"/> items.</summary>
    private TSut CreateSut() => CreateSut(DefaultItemCount);

    /// <summary>
    /// Returns the source items used to feed the loader. Must return at least 5 items.
    /// </summary>
    protected abstract IReadOnlyList<TItem> CreateSourceItems();

    /// <summary>
    /// Returns the items collected by <paramref name="sut"/> during the most recent load, or
    /// <see langword="null"/> if the loader does not collect items.
    /// </summary>
    /// <remarks>
    /// Override to return the loader's collected snapshot (for example
    /// <c>sut.GetCollectedItems()</c>). When this returns <see langword="null"/> the
    /// no-state-leak test treats the loader as non-collecting and skips its collection check.
    /// </remarks>
    /// <param name="sut">The system under test, after a load has completed.</param>
    protected abstract IReadOnlyList<TItem>? TryGetLoadedItems(TSut sut);



    // ------------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that loading the same source twice into the same instance processes all input
    /// items on both runs.
    /// </summary>
    [Fact]
    public async Task LoadAsync_when_called_twice_processes_all_items_both_times_Async()
    {
        var sut = CreateSut();
        var source = CreateSourceItems();

        var firstException = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(source.ToAsyncEnumerable())
        ).ConfigureAwait(false);

        Assert.Null(firstException);

        var firstLoaded = TryGetLoadedItems(sut);
        if (firstLoaded is not null)
        {
            Assert.Equal(source.Count, firstLoaded.Count);
        }

        var secondException = await Record.ExceptionAsync
        (
            () => sut.LoadAsync(source.ToAsyncEnumerable())
        ).ConfigureAwait(false);

        Assert.Null(secondException);

        var secondLoaded = TryGetLoadedItems(sut);
        if (secondLoaded is not null)
        {
            Assert.Equal(source.Count, secondLoaded.Count);
        }
    }



    /// <summary>
    /// Verifies that a collecting loader carries no state across runs: after a second load, the
    /// collected snapshot contains only the second run's items, with no carryover from the first.
    /// </summary>
    [Fact]
    public async Task LoadAsync_when_called_twice_no_state_leaks_Async()
    {
        var sut = CreateSut();
        var source = CreateSourceItems();

        await sut.LoadAsync(source.ToAsyncEnumerable()).ConfigureAwait(false);

        var afterFirstRun = TryGetLoadedItems(sut);
        if (afterFirstRun is null)
        {
            // Non-collecting loader: there is no snapshot to inspect for carryover.
            return;
        }

        await sut.LoadAsync(source.ToAsyncEnumerable()).ConfigureAwait(false);

        var afterSecondRun = TryGetLoadedItems(sut);

        Assert.NotNull(afterSecondRun);
        Assert.Equal(source, afterSecondRun);
    }
}
