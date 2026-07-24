#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wolfgang.Etl.TestKit;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Guards the doubles' per-item hot path against allocation regressions (#136).
/// </summary>
/// <remarks>
/// The doubles exist to feed benchmarks of the real ETL code, so any per-item
/// allocation they introduce shows up as noise in those measurements. The
/// enumeration hot paths below are intended to be <b>zero-allocation per item</b>
/// (a constant, one-time setup cost independent of the item count):
/// <list type="bullet">
///   <item><description><see cref="TestExtractor{T}.ExtractWorkerAsync"/> via <c>ExtractAsync()</c></description></item>
///   <item><description><see cref="TestLoader{T}.LoadWorkerAsync"/> via <c>LoadAsync(...)</c> with <c>collectItems: false</c></description></item>
///   <item><description><see cref="TestTransformer{T}.TransformWorkerAsync"/> via <c>TransformAsync(...)</c></description></item>
/// </list>
/// Each test measures allocation over N and 10·N items and asserts the marginal
/// per-item allocation stays below a small threshold — a real regression (a
/// per-item <c>List.Add</c>, boxing an <c>int</c> at ~24 B/item, an LINQ closure)
/// pushes the marginal cost far above it, while one-time setup and background
/// noise cancel out of the delta. Measured baseline: ~0 B/item on all three.
/// Guarded to net6.0+ where <see cref="GC.GetTotalAllocatedBytes(bool)"/> exists.
/// </remarks>
public sealed class AllocationRegressionTests
{
    // Real per-item regressions allocate >= 24 bytes/item (boxing) or an amortized
    // slab (List growth). 4 bytes/item is comfortably above measurement noise yet
    // an order of magnitude below any genuine per-item allocation.
    private const double MaxBytesPerItem = 4.0;

    private const int BaseCount = 20_000;

    [Fact]
    public async Task ExtractAsync_does_not_allocate_per_item()
    {
        await AssertZeroAllocPerItem(async count =>
        {
            var extractor = new TestExtractor<int>(new int[count]);
            var seen = 0;
            await foreach (var _ in extractor.ExtractAsync())
            {
                seen++;
            }

            return seen;
        });
    }

    [Fact]
    public async Task LoadAsync_does_not_allocate_per_item()
    {
        await AssertZeroAllocPerItem(async count =>
        {
            var loader = new TestLoader<int>(collectItems: false);
            await loader.LoadAsync(Range(count));
            return count;
        });
    }

    [Fact]
    public async Task TransformAsync_does_not_allocate_per_item()
    {
        await AssertZeroAllocPerItem(async count =>
        {
            var transformer = new TestTransformer<int>();
            var seen = 0;
            await foreach (var _ in transformer.TransformAsync(Range(count)))
            {
                seen++;
            }

            return seen;
        });
    }

    // Measures the marginal per-item allocation as (alloc(10N) - alloc(N)) / (9N),
    // which cancels the one-time setup cost, and takes the minimum across a few
    // runs to shed transient background-allocation noise. Fails if the marginal
    // per-item allocation exceeds the threshold.
    private static async Task AssertZeroAllocPerItem(Func<int, Task<int>> run)
    {
        // Warm up so JIT / first-run allocations do not land in the measurement.
        await run(BaseCount);
        await run(BaseCount * 10);

        var best = double.MaxValue;

        for (var attempt = 0; attempt < 3; attempt++)
        {
            var small = await Measure(run, BaseCount);
            var large = await Measure(run, BaseCount * 10);

            var perItem = (double)(large - small) / (BaseCount * 10 - BaseCount);
            best = Math.Min(best, perItem);
        }

        Assert.True
        (
            best < MaxBytesPerItem,
            $"Per-item allocation {best:F3} B exceeds the {MaxBytesPerItem} B/item budget — the hot path regressed to allocating per item."
        );
    }

    private static async Task<long> Measure(Func<int, Task<int>> run, int count)
    {
        var before = GC.GetTotalAllocatedBytes(precise: true);
        await run(count);
        return GC.GetTotalAllocatedBytes(precise: true) - before;
    }

    private static async IAsyncEnumerable<int> Range(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return i;
        }

        await Task.CompletedTask;
    }
}
#endif
