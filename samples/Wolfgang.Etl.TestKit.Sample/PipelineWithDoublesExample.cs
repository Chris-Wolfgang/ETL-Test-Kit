using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;
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

    [Fact]
    public async Task Compose_extract_transform_load_with_EtlPipeline()
    {
        var readings = SampleReadings(3).ToList();

        // Compose the doubles with the Abstractions 0.16 fluent pipeline: a source
        // (TestExtractor), a stage (TestTransformer, pass-through here), and a sink
        // (TestLoader) that captures the result for the assertion.
        using var extractor   = new TestExtractor<TemperatureReading>(readings);
        using var transformer = new TestTransformer<TemperatureReading>();
        using var loader      = new TestLoader<TemperatureReading>(collectItems: true);

        await EtlPipeline
            .Create()
            .From(extractor)
            .Through(transformer)
            .To(loader)
            .RunAsync();

        Assert.Equal(readings, loader.GetCollectedItems());
    }

    [Fact]
    public async Task Skip_bad_items_in_a_pipeline_with_the_error_hook()
    {
        // A Faulty* double with SkipErrors() exercises the Abstractions 0.18 error hook
        // inside a real pipeline: the bad item is discarded and counted as an error,
        // and the run completes, so the loader still receives the survivors.
        using var extractor   = new TestExtractor<TemperatureReading>(SampleReadings(5));
        using var transformer = new FaultyTransformer<TemperatureReading>()
            .ThrowAt(2, new InvalidOperationException("bad reading"))
            .SkipErrors();
        using var loader      = new TestLoader<TemperatureReading>(collectItems: true);

        await EtlPipeline
            .Create()
            .From(extractor)
            .Through(transformer)
            .To(loader)
            .RunAsync();

        Assert.Equal(4, loader.GetCollectedItems()!.Count); // one bad reading skipped
        Assert.Equal(1, transformer.CurrentErrorItemCount);
    }
}
