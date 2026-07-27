using System;

namespace Wolfgang.Etl.TestKit.Sample;

/// <summary>
/// A record produced by the <see cref="WeatherStationExtractor"/> — a small
/// realistic domain type for the sample. A <c>record</c> gives it the value
/// equality the contract-test base classes rely on when comparing expected vs
/// actual items.
/// </summary>
public sealed record TemperatureReading(DateTimeOffset Timestamp, double Celsius);
