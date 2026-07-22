using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Wolfgang.Etl.TestKit.Sample;

/// <summary>
/// The other use case: when you are testing something else (a pipeline, an
/// orchestrator, error handling) and just need cheap, ready-made ETL components,
/// use the kit's <em>doubles</em> as stand-ins — feed known data with
/// <see cref="TestExtractor{T}"/>, capture output with <see cref="TestLoader{T}"/>,
/// and inject failures with the <c>Faulty*</c> doubles.
/// </summary>
public sealed class PipelineWithDoublesExample
{
    private static readonly DateTimeOffset BaseTime =
        new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static IEnumerable<TemperatureReading> SampleReadings(int count) =>
        Enumerable.Range(0, count)
            .Select(i => new TemperatureReading(BaseTime.AddHours(i), 20.0 + i));

    [Fact]
    public async Task Capture_pipeline_output_with_TestLoader()
    {
        var readings = SampleReadings(3).ToList();

        // TestExtractor feeds known data; TestLoader captures whatever the
        // pipeline pushed so the test can assert on it.
        using var extractor = new TestExtractor<TemperatureReading>(readings);
        using var loader = new TestLoader<TemperatureReading>(collectItems: true);

        await loader.LoadAsync(extractor.ExtractAsync());

        Assert.Equal(readings, loader.GetCollectedItems());
    }

    [Fact]
    public async Task Verify_error_handling_with_a_Faulty_double()
    {
        // FaultyExtractor throws on demand at a chosen position, so a consumer
        // can prove their pipeline surfaces (or recovers from) mid-stream errors.
        using var extractor = new FaultyExtractor<TemperatureReading>(SampleReadings(5))
            .ThrowAt(2, new InvalidOperationException("sensor dropped out"));

        var seen = new List<TemperatureReading>();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var reading in extractor.ExtractAsync())
            {
                seen.Add(reading);
            }
        });

        Assert.Equal("sensor dropped out", ex.Message);
        Assert.Equal(2, seen.Count); // two readings surfaced before the failure
    }
}
