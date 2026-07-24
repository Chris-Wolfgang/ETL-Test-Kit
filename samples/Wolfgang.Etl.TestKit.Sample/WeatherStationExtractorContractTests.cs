using System;
using System.Collections.Generic;
using System.Linq;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;

namespace Wolfgang.Etl.TestKit.Sample;

/// <summary>
/// The headline use case for the kit: verify a hand-written custom extractor
/// (<see cref="WeatherStationExtractor"/>) against the full extractor contract by
/// subclassing <see cref="ExtractorBaseContractTests{TSut, TItem, TProgress}"/>
/// and filling in three factory methods. Inheriting the base class runs its whole
/// suite — enumeration, cancellation, progress, skip/max windowing, idempotency —
/// against your extractor for free.
/// </summary>
public sealed class WeatherStationExtractorContractTests
    : ExtractorBaseContractTests<WeatherStationExtractor, TemperatureReading, Report>
{
    private static readonly DateTimeOffset BaseTime =
        new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // A deterministic reading generator so CreateSut and CreateExpectedItems agree.
    private static IReadOnlyList<TemperatureReading> CreateReadings(int count) =>
        Enumerable.Range(0, count)
            .Select(i => new TemperatureReading(BaseTime.AddHours(i), 20.0 + i))
            .ToList();

    /// <inheritdoc/>
    protected override WeatherStationExtractor CreateSut(int itemCount) =>
        new WeatherStationExtractor(CreateReadings(itemCount));

    /// <inheritdoc/>
    protected override IReadOnlyList<TemperatureReading> CreateExpectedItems() =>
        CreateReadings(5);

    /// <inheritdoc/>
    protected override WeatherStationExtractor CreateSutWithTimer(IProgressTimer timer) =>
        new WeatherStationExtractor(CreateReadings(5), timer);
}
