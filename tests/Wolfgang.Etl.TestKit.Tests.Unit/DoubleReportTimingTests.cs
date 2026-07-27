using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
using Xunit;

namespace Wolfgang.Etl.TestKit.Tests.Unit;

/// <summary>
/// Verifies that the doubles surface the base class's timing instrumentation (Abstractions
/// 0.14.0) in the <see cref="Report"/> they build — <see cref="Report.StartedAt"/> and
/// <see cref="Report.Elapsed"/>, which in turn drive <see cref="Report.ItemsPerSecond"/>. The
/// <c>CreateProgressReport</c> override is byte-identical across all six doubles, so
/// <see cref="TestExtractor{T}"/> stands in as the representative.
/// </summary>
public class DoubleReportTimingTests
{
    [Fact]
    public void CreateProgressReport_before_any_run_has_no_StartedAt()
    {
        var sut = new ExposedTestExtractor<int>(new List<int> { 1, 2, 3 });

        var report = sut.GetProgressReport();

        Assert.Null(report.StartedAt);
    }

    [Fact]
    public async Task CreateProgressReport_after_a_run_surfaces_StartedAt_and_Elapsed()
    {
        var sut = new ExposedTestExtractor<int>(new List<int> { 1, 2, 3 });

        await sut.ExtractAsync().ToListAsync();
        var report = sut.GetProgressReport();

        Assert.NotNull(report.StartedAt);
        Assert.True(report.Elapsed >= TimeSpan.Zero);
        Assert.Equal(3, report.CurrentItemCount);
    }

    [Fact]
    public async Task CreateProgressReport_after_a_run_computes_a_non_negative_throughput()
    {
        var sut = new ExposedTestExtractor<int>(Enumerable.Range(0, 50).ToList());

        await sut.ExtractAsync().ToListAsync();
        var report = sut.GetProgressReport();

        Assert.True(report.ItemsPerSecond >= 0);
    }

    private sealed class ExposedTestExtractor<T> : TestExtractor<T>
        where T : notnull
    {
        public ExposedTestExtractor(IEnumerable<T> items)
            : base(items)
        {
        }

        public Report GetProgressReport() => CreateProgressReport();
    }
}
