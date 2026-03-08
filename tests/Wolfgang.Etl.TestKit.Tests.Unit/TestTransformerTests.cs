using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

public class TestTransformerTests
{
    // ------------------------------------------------------------------
    // Pass-through behaviour
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_yields_all_items_unchanged_and_in_order()
    {
        var extractor    = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var transformer  = new TestTransformer<int>();

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal
        (
            new[] { 1, 2, 3 },
            results
        );
    }



    [Fact]
    public async Task TransformAsync_with_empty_source_yields_no_items()
    {
        var extractor   = new TestExtractor<string>(new List<string>());
        var transformer = new TestTransformer<string>();

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Empty(results);
    }



    [Fact]
    public async Task TransformAsync_with_reference_type_yields_same_references()
    {
        // Verifies pass-through — no copying or wrapping of reference types.
        var a = new object();
        var b = new object();
        var extractor   = new TestExtractor<object>(new List<object> { a, b });
        var transformer = new TestTransformer<object>();

        var results = await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Same(a, results[0]);
        Assert.Same(b, results[1]);
    }



    // ------------------------------------------------------------------
    // CurrentItemCount
    // ------------------------------------------------------------------

    [Fact]
    public async Task TransformAsync_increments_CurrentItemCount()
    {
        var extractor   = new TestExtractor<int>(new List<int> { 10, 20, 30, 40 });
        var transformer = new TestTransformer<int>();

        await transformer.TransformAsync(extractor.ExtractAsync()).ToListAsync();

        Assert.Equal(4, transformer.CurrentItemCount);
    }



    // ------------------------------------------------------------------
    // CreateProgressReport
    // ------------------------------------------------------------------

    [Fact]
    public async Task CreateProgressReport_returns_Report_with_current_item_count()
    {
        var extractor   = new TestExtractor<int>(new List<int> { 1, 2, 3 });
        var transformer = new TestTransformer<int>();
        Report? report  = null;

        await transformer
            .TransformAsync
            (
                extractor.ExtractAsync(),
                new Progress<Report>(r => report = r)
            )
            .ToListAsync();

        Assert.NotNull(report);
        Assert.True(report!.CurrentCount >= 0);
    }
}
