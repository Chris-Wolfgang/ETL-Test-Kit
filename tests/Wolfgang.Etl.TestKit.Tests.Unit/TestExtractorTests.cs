using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class TestExtractorTests
{
    // ------------------------------------------------------------------
    // IEnumerable<T> constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_with_enumerable_when_items_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractor<int>((IEnumerable<int>)null!)
        );
    }



    // ------------------------------------------------------------------
    // IEnumerator<T> constructor — argument validation
    // ------------------------------------------------------------------

    [Fact]
    public void Constructor_with_enumerator_when_enumerator_is_null_throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>
        (
            () => new TestExtractor<int>((IEnumerator<int>)null!)
        );
    }



    // ------------------------------------------------------------------
    // IEnumerable<T> — extraction
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_enumerable_yields_all_items_in_order()
    {
        var items = new List<int> { 1, 2, 3 };
        var extractor = new TestExtractor<int>(items);

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal
        (
            new[] { 1, 2, 3 },
            results
        );
    }



    [Fact]
    public async Task ExtractAsync_with_empty_enumerable_yields_no_items()
    {
        var extractor = new TestExtractor<int>(new List<int>());

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Empty(results);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerable_increments_CurrentItemCount()
    {
        var extractor = new TestExtractor<int>(new List<int> { 10, 20, 30 });

        await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(3, extractor.CurrentItemCount);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerable_can_be_reused_across_multiple_runs()
    {
        var items = new List<string> { "a", "b" };
        var extractor = new TestExtractor<string>(items);

        var first = await extractor.ExtractAsync().ToListAsync();
        var second = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(first, second);
    }



    // ------------------------------------------------------------------
    // IEnumerator<T> — extraction
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_enumerator_yields_all_items_in_order()
    {
        var extractor = new TestExtractor<int>(GenerateInts(3));

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Equal
        (
            new[] { 0, 1, 2 },
            results
        );
    }



    [Fact]
    public async Task ExtractAsync_with_empty_enumerator_yields_no_items()
    {
        var extractor = new TestExtractor<int>(GenerateInts(0));

        var results = await extractor.ExtractAsync().ToListAsync();

        Assert.Empty(results);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerator_increments_CurrentItemCount()
    {
        var extractor = new TestExtractor<int>(GenerateInts(5));

        await extractor.ExtractAsync().ToListAsync();

        Assert.Equal(5, extractor.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // Cancellation
    // ------------------------------------------------------------------

    [Fact]
    public async Task ExtractAsync_with_enumerable_respects_cancellation()
    {
        var items = new List<int> { 1, 2, 3, 4, 5 };
        var extractor = new TestExtractor<int>(items);
        using var cts = new CancellationTokenSource();

        var results = new List<int>();
        await Assert.ThrowsAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var item in extractor.ExtractAsync(cts.Token))
                {
                    results.Add(item);
                    if (results.Count == 2)
                    {
                        cts.Cancel();
                    }
                }
            }
        );

        Assert.Equal(2, results.Count);
    }



    [Fact]
    public async Task ExtractAsync_with_enumerator_respects_cancellation()
    {
        var extractor = new TestExtractor<int>(GenerateInts(100));
        using var cts = new CancellationTokenSource();

        var results = new List<int>();
        await Assert.ThrowsAsync<OperationCanceledException>
        (
            async () =>
            {
                await foreach (var item in extractor.ExtractAsync(cts.Token))
                {
                    results.Add(item);
                    if (results.Count == 3)
                    {
                        cts.Cancel();
                    }
                }
            }
        );

        Assert.Equal(3, results.Count);
    }



    // ------------------------------------------------------------------
    // CreateProgressReport
    // ------------------------------------------------------------------

    [Fact]
    public async Task CreateProgressReport_returns_Report_reflecting_CurrentItemCount()
    {
        var extractor = new ExposedTestExtractor<int>(new List<int> { 1, 2, 3 });

        await extractor.ExtractAsync().ToListAsync();

        var report = extractor.GetProgressReport();

        Assert.Equal(3, report.CurrentCount);
    }



    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static IEnumerator<int> GenerateInts(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return i;
        }
    }


    private sealed class ExposedTestExtractor<T>(IEnumerable<T> items) : TestExtractor<T>(items)
    {
        public Report GetProgressReport() => CreateProgressReport();
    }
}
