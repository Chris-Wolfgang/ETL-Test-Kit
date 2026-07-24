using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Sample;

/// <summary>
/// A worked example of a <em>custom</em> extractor a consumer of this library
/// might write: it reads <see cref="TemperatureReading"/> records from an
/// in-memory source (standing in for a real sensor log / API / file). It is
/// deliberately hand-written — NOT one of the kit's own doubles — to show the
/// exact <see cref="ExtractorBase{TSource, TProgress}"/> pattern a consumer must
/// implement, including <see cref="ExtractorBase{TSource, TProgress}.SkipItemCount"/> /
/// <see cref="ExtractorBase{TSource, TProgress}.MaximumItemCount"/> windowing and
/// injectable progress-timer wiring, so that
/// <see cref="Xunit.ExtractorBaseContractTests{TSut, TItem, TProgress}"/> passes
/// against it (see WeatherStationExtractorContractTests).
/// </summary>
public sealed class WeatherStationExtractor : ExtractorBase<TemperatureReading, Report>
{
    private readonly IEnumerable<TemperatureReading> _readings;

    // Injected timer support: a consumer only needs this if they want their
    // extractor to be verifiable with a deterministic ManualProgressTimer.
    private readonly IProgressTimer? _progressTimer;
    private bool _progressTimerWired;
    private Action? _elapsedHandler;

    /// <summary>Creates an extractor over the supplied readings.</summary>
    public WeatherStationExtractor(IEnumerable<TemperatureReading> readings)
    {
        _readings = readings ?? throw new ArgumentNullException(nameof(readings));
    }

    /// <summary>
    /// Creates an extractor over the supplied readings that reports progress
    /// through the injected <paramref name="timer"/> — the seam the contract
    /// tests drive with a deterministic timer.
    /// </summary>
    public WeatherStationExtractor(IEnumerable<TemperatureReading> readings, IProgressTimer timer)
        : this(readings)
    {
        _progressTimer = timer ?? throw new ArgumentNullException(nameof(timer));
    }

    /// <inheritdoc/>
    protected override Report CreateProgressReport() => new(CurrentItemCount);

    /// <inheritdoc/>
    protected override IProgressTimer CreateProgressTimer(IProgress<Report> progress)
    {
        if (_progressTimer is null)
        {
            return base.CreateProgressTimer(progress);
        }

        // Guard against re-wiring the handler if CreateProgressTimer is called
        // more than once for the injected timer.
        if (!_progressTimerWired)
        {
            _progressTimerWired = true;
            _elapsedHandler = () => progress.Report(CreateProgressReport());
            _progressTimer.Elapsed += _elapsedHandler;
        }

        return _progressTimer;
    }

    /// <inheritdoc/>
    protected override async IAsyncEnumerable<TemperatureReading> ExtractWorkerAsync
    (
        [EnumeratorCancellation] CancellationToken token
    )
    {
        token.ThrowIfCancellationRequested();

        // Honour the async-iterator contract even though the source is synchronous.
        await Task.Yield();

        foreach (var reading in _readings)
        {
            token.ThrowIfCancellationRequested();

            // SkipItemCount: drop the leading items the caller asked to skip.
            if (CurrentSkippedItemCount < SkipItemCount)
            {
                IncrementCurrentSkippedItemCount();
                continue;
            }

            // MaximumItemCount: stop once the cap is reached.
            if (CurrentItemCount >= MaximumItemCount)
            {
                yield break;
            }

            // Count exactly once per yielded item.
            IncrementCurrentItemCount();
            yield return reading;
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        // The injected timer is owned by the caller, so only unsubscribe — never
        // dispose it here.
        if (disposing && _progressTimer is not null && _elapsedHandler is not null)
        {
            _progressTimer.Elapsed -= _elapsedHandler;
            _elapsedHandler = null;
        }

        base.Dispose(disposing);
    }
}
