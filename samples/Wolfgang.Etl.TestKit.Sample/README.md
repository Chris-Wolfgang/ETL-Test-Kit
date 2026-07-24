# Wolfgang.Etl.TestKit — worked sample

A runnable "real consumer" sample (#116) showing the two ways you use the kit.
Because it is a live xUnit project, everything here is compile- and
behaviour-verified in CI — documentation that cannot silently rot.

## 1. Verify your own ETL component with the contract tests

[`WeatherStationExtractor`](WeatherStationExtractor.cs) is a hand-written custom
extractor — the kind you'd write over a real sensor log, API, or file. It
implements the `ExtractorBase<T, Report>` pattern in full: `ExtractWorkerAsync`
with `SkipItemCount` / `MaximumItemCount` windowing, `CreateProgressReport`, and
injectable progress-timer wiring.

[`WeatherStationExtractorContractTests`](WeatherStationExtractorContractTests.cs)
verifies it by subclassing `ExtractorBaseContractTests<…>` and filling in three
factory methods (`CreateSut`, `CreateExpectedItems`, `CreateSutWithTimer`).
Inheriting the base runs its entire suite — enumeration, cancellation, progress,
skip/max, idempotency — against your extractor for free.

## 2. Use the doubles as stand-ins in your own tests

When you're testing something else and just need cheap ETL parts,
[`PipelineWithDoublesExample`](PipelineWithDoublesExample.cs) shows:

- `TestExtractor<T>` to feed known data and `TestLoader<T>` to capture output;
- `FaultyExtractor<T>.ThrowAt(...)` to inject a mid-stream failure and prove your
  pipeline surfaces (or recovers from) errors.

## Run it

```bash
dotnet test samples/Wolfgang.Etl.TestKit.Sample
```
