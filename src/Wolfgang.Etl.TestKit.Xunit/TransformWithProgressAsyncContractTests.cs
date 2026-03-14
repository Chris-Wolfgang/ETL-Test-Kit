using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Xunit;

/// <summary>
/// Abstract base class providing xUnit contract tests for any type that implements
/// <see cref="ITransformWithProgressAsync{TSource, TDestination, TProgress}"/>.
/// </summary>
/// <typeparam name="TSut">
/// The type under test. Must implement
/// <see cref="ITransformWithProgressAsync{TSource, TDestination, TProgress}"/>.
/// </typeparam>
/// <typeparam name="TItem">The type of item the transformer consumes and produces.</typeparam>
/// <typeparam name="TProgress">The type of the progress report.</typeparam>
/// <example>
/// <code>
/// public class MyTransformerProgressContractTests
///     : TransformWithProgressAsyncContractTests&lt;MyTransformer, MyRecord, MyProgress&gt;
/// {
///     protected override MyTransformer CreateSut() => new MyTransformer();
///
///     protected override IReadOnlyList&lt;MyRecord&gt; CreateExpectedItems() =>
///         new List&lt;MyRecord&gt; { new("a"), new("b"), new("c") };
/// }
/// </code>
/// </example>
public abstract class TransformWithProgressAsyncContractTests<TSut, TItem, TProgress>
    where TSut : ITransformWithProgressAsync<TItem, TItem, TProgress>
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


    /// <summary>Returns the expected items for the default SUT — integers 1 to <see cref="DefaultItemCount"/>.</summary>
    private IReadOnlyList<TItem> CreateExpectedItems() =>
        Enumerable.Range(1, DefaultItemCount).Cast<TItem>().ToList();



    // ------------------------------------------------------------------
    // Tests
    // ------------------------------------------------------------------

    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// throws <see cref="ArgumentNullException"/> when <c>progress</c> is <see langword="null"/>.
    /// </summary>
    [Fact]
    public void TransformAsync_with_null_progress_throws_ArgumentNullException()
    {
        var sut = CreateSut();

        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = sut.TransformAsync(AsyncEnumerable.Empty<TItem>(), (IProgress<TProgress>)null!);
        });
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// yields the expected items when a valid progress instance is supplied.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_yields_expected_items()
    {
        var sut = CreateSut();
        var expected = CreateExpectedItems();
        var progress = new Progress<TProgress>();

        var actual = await sut.TransformAsync(expected.ToAsyncEnumerable(), progress).ToListAsync();

        Assert.Equal(expected, actual);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(IAsyncEnumerable&lt;TItem&gt;, IProgress&lt;TProgress&gt;)</c>
    /// invokes the progress callback at least once during transformation.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_invokes_callback_at_least_once()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.TransformAsync(CreateExpectedItems().ToAsyncEnumerable(), progress).ToListAsync();

        Assert.True(callbackCount >= 1);
    }



    /// <summary>
    /// Verifies that <c>TransformAsync(items, IProgress&lt;TProgress&gt;)</c> invokes the
    /// progress callback at least once even when the source is empty.
    /// </summary>
    [Fact]
    public async Task TransformAsync_with_progress_and_empty_source_invokes_callback_at_least_once()
    {
        var sut = CreateSut();
        var callbackCount = 0;
        var progress = new SynchronousProgress<TProgress>(_ => callbackCount++);

        await sut.TransformAsync(AsyncEnumerable.Empty<TItem>(), progress).ToListAsync();

        Assert.True(callbackCount >= 1);
    }
}
