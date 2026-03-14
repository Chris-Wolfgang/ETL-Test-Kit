using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that implements
/// <see cref="ITransformAsync{TSource, TDestination}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement <see cref="ITransformAsync{TSource, TDestination}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the transformer consumes and produces.</typeparam>
/// <remarks>
/// Inherit from this class and implement the abstract factory methods to verify that
/// your transformer correctly satisfies the <see cref="ITransformAsync{TSource, TDestination}"/>
/// contract.
/// </remarks>
/// <example>
/// <code>
/// public class MyTransformerContractTests
///     : TransformAsyncContractTests&lt;MyTransformer, MyRecord&gt;
/// {
///     protected override MyTransformer CreateSut() => new MyTransformer();
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c") };
/// }
/// </code>
/// </example>
public abstract class TransformAsyncContractTests<TSut, TItem>
    where TSut : ITransformAsync<TItem, TItem>
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


    /// <summary>Returns the expected items for the default SUT — integers 1 to <see cref="DefaultItemCount"/>.</summary>
    private IReadOnlyList<TItem> CreateExpectedItems() =>
        Enumerable.Range(1, DefaultItemCount).Cast<TItem>().ToList();



    // ------------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;)</c> returns a
    /// non-null sequence.
    /// </summary>
    [Fact]
    public void TransformAsync_returns_non_null_sequence()
    {
        var sut = CreateSut();
        var result = sut.TransformAsync(AsyncEnumerable.Empty<TItem>());
        Assert.NotNull(result);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;)</c> yields the
    /// expected items in order.
    /// </summary>
    [Fact]
    public async Task TransformAsync_yields_expected_items_in_order()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();

        var actual = await sut.TransformAsync(expected.ToAsyncEnumerable()).ToListAsync();

        Assert.Equal(expected, actual);
    }
}
