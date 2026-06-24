using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing opt-in xUnit idempotency contract tests for any type that
/// implements <see cref="ITransformAsync{TSource, TDestination}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement <see cref="ITransformAsync{TSource, TDestination}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the transformer consumes and produces.</typeparam>
/// <remarks>
/// <para>
/// <b>This base class is opt-in.</b> Inherit from it <em>only</em> if your transformer supports
/// being run more than once on the <em>same</em> instance and produces the same output for the
/// same input each time, with no accumulated side effects. Single-use components — for example a
/// transformer that maintains running state intended to span a single pipeline — must
/// <em>not</em> inherit this class.
/// </para>
/// <para>
/// Each test calls <see cref="CreateSut(int)"/> exactly once and then transforms the same input
/// twice, asserting that the second run is unaffected by the first.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyTransformerIdempotencyTests
///     : IdempotentTransformerContractTests&lt;MyTransformer, MyRecord&gt;
/// {
///     protected override MyTransformer CreateSut(int itemCount) =>
///         new MyTransformer();
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c"), new("d"), new("e") };
/// }
/// </code>
/// </example>
public abstract class IdempotentTransformerContractTests<TSut, TItem>
    where TSut : ITransformAsync<TItem, TItem>
    where TItem : notnull
{
    // NOTE: A companion test asserting that CurrentItemCount resets to zero between runs is
    // intentionally deferred. Today CurrentItemCount is cumulative across runs on the same
    // instance; whether it should reset is under decision in ETL-Abstractions#246. For the
    // same reason these tests rely on the default MaximumItemCount — setting it would cause a
    // second run to immediately hit the cumulative limit and yield nothing.

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
    /// Returns the items used as input to the transformer and expected back unchanged. Must
    /// return at least 5 items.
    /// </summary>
    protected abstract IReadOnlyList<TItem> CreateExpectedItems();



    // ------------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that transforming the same input twice on the same instance yields identical
    /// output, in the same order, on both runs.
    /// </summary>
    [Fact]
    public async Task TransformAsync_when_called_twice_yields_identical_items_Async()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var firstRun = await sut
            .TransformAsync(expected.ToAsyncEnumerable())
            .ToListAsync()
            .ConfigureAwait(false);

        var secondRun = await sut
            .TransformAsync(expected.ToAsyncEnumerable())
            .ToListAsync()
            .ConfigureAwait(false);

        Assert.Equal(expected, firstRun);
        Assert.Equal(firstRun, secondRun);
        Assert.Equal(expected, secondRun);
    }



    /// <summary>
    /// Verifies that a second transform run is not influenced by the first: the second run's
    /// output shows no growth, duplication, or other accumulated side effect.
    /// </summary>
    [Fact]
    public async Task TransformAsync_when_called_twice_no_accumulated_side_effects_Async()
    {
        var sut = CreateSut();
        var input = CreateExpectedItems();

        var firstRun = await sut
            .TransformAsync(input.ToAsyncEnumerable())
            .ToListAsync()
            .ConfigureAwait(false);

        var secondRun = await sut
            .TransformAsync(input.ToAsyncEnumerable())
            .ToListAsync()
            .ConfigureAwait(false);

        Assert.Equal(firstRun.Count, secondRun.Count);
        Assert.Equal(input.Count, secondRun.Count);
        Assert.Equal(firstRun, secondRun);
    }
}
