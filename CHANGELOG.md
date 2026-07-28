# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `EtlPipelineContractTests<TItem, TProgress>` — an opt-in xUnit contract-test base that composes a
  source and a loader sink into the `Wolfgang.Etl.Abstractions` 0.16 `EtlPipeline`
  (`EtlPipeline.Create().From(...).To(...).RunAsync()`) and verifies the run delivers every source
  item and reports each record as extracted and loaded via `EtlPipelineProgress`. The derived test
  supplies `CreateSourceItems()`, `CreateSink()`, and `GetLoadedItems()`; the harness-managed `Sink`
  property carries the composed loader so the read-back needs no null-argument validation (#256).
- `SnapshotTestLoader<T>` — a capture-only loader double that records every item a pipeline loads and
  renders them as a single deterministic, diff-friendly `Snapshot` string (one formatted line per
  item, joined by `\n`) plus a `LoadedItems` list. Designed to hand off to an approval / snapshot
  framework such as [Verify](https://github.com/VerifyTests/Verify): it does no file I/O and takes
  **no dependency on any snapshot framework**, so referencing `Wolfgang.Etl.TestKit` never pulls one
  in. The default constructor formats each item with `ToString()` (diff-friendly for `record` types);
  a `Func<T, string>` constructor lets you project the fields under test and scrub non-deterministic
  values (timestamps, GUIDs, auto-increment IDs). `SkipItemCount` / `MaximumItemCount` bound the
  capture. README documents the fleet snapshot convention (dedicated single-TFM `*.Tests.Snapshot`
  project, `Verify.Xunit`, `.verified.txt` golden files under `Snapshots/`). (#11, closes #129)

### Changed

### Deprecated

### Removed

### Fixed

### Security

## [0.12.0] - Unreleased

New opt-in contract-test bases. New public API only — no breaking change.

### Added

- `AllocationBudgetContractTests<TSut>` — an opt-in xUnit contract-test base that asserts a
  repeatable operation's hot path stays within a declared per-item allocation budget
  (`MaxBytesPerItem`, default 0 = allocation-free). Measures the *marginal* allocation
  (`(alloc(10N) - alloc(N)) / 9N`), GC-settled and min-of-attempts, so one-time setup does not
  count. Uses the process-wide `GC.GetTotalAllocatedBytes`, so derived tests **must be
  serialized** (documented; a `[Collection("Allocation")]` example is provided). Skips on
  frameworks without the counter (net462 / netstandard2.0). (#245)
- `DelayingExtractor<T>` — an extractor double that waits a configurable delay (a fixed `TimeSpan` or
  a per-index `Func<int, TimeSpan>`) before yielding each item, simulating a latent / backpressured
  source. The delay is awaited with `Task.Delay(..., token)`, so a cancel interrupts the wait and the
  extractor stops promptly. Honours `SkipItemCount` / `MaximumItemCount`. (#264)
- `CancellationContractTests<TSut>` + `CancellationOutcome` — an opt-in xUnit contract-test base that
  verifies a stage cancels *promptly*: a mid-stream cancel stops within `PromptStopSlack` items (not a
  full drain) and throws `OperationCanceledException`, and an already-cancelled token processes nothing.
  The derived class drives its own stage and reports a `CancellationOutcome` (no SUT is passed to the
  override). (#264)

## [0.11.0] - 2026-07-26

Adopts the `Wolfgang.Etl.Abstractions` 0.18.0 per-item **error hook** in the test doubles
and adds contract-test bases covering the error hook and the disposability guarantees, plus
surfaces the base classes' timing instrumentation in the doubles' progress reports. New
public API only — no breaking change.

### Added

- **Error hook (Abstractions 0.18.0).** `FaultyExtractor<T>`, `FaultyLoader<T>`, and
  `FaultyTransformer<T>` gain `SkipErrors()`, `HandleErrorsWith(policy)`, and `CapturedErrors`:
  an injected `ThrowAt` fault is routed through the base `HandleItemError` hook, so it is
  discarded and counted as an error (`CurrentErrorItemCount`) and the run continues on
  `ItemErrorAction.Skip`, or re-thrown on `Abort`. With no policy configured a fault still
  propagates (fail-fast), unchanged.
- `ErrorHandlingContractTests<TSut>` + `ErrorHandlingOutcome` — an opt-in xUnit contract-test
  base verifying a stage's error hook: a `Skip` policy completes the run and counts the failure
  as an error kept *distinct* from the intentional-skip count; an `Abort` policy re-throws and
  counts no error.
- `DisposableStageContractTests<TSut>` — an opt-in xUnit contract-test base verifying a stage
  throws `ObjectDisposedException` after `Dispose()`/`DisposeAsync()` (the 0.17 use-after-dispose
  guard) and that disposing twice is a harmless no-op.
- Counter contract tests on `ExtractorBaseContractTests`, `LoaderBaseContractTests`, and
  `TransformerBaseContractTests`: `CurrentItemCount` / `CurrentSkippedItemCount` /
  `CurrentErrorItemCount` default-to-zero and skip-count-tracking assertions, inherited free by
  every downstream contract-test class (#248).
- "No over-read" contract tests (#49) on all three base classes: a stage must stop pulling from
  its source once `MaximumItemCount` is reached (≤ M+1 reads) or the run is cancelled, and a
  pre-cancelled token must read nothing. Extractors opt in by overriding the new
  `CreateSutOverSource` factory (a no-op by default for extractors whose source is not an
  injectable sequence).

### Changed

- Built against `Wolfgang.Etl.Abstractions` 0.17.0 → **0.18.1**.
- The doubles build their progress `Report` via the new
  `Report(int, DateTimeOffset?, TimeSpan, int?)` constructor (Abstractions 0.18.1) instead of
  the object-initializer form, so setting the timing/total values is safe cross-assembly on
  every target framework (see Fixed).
- The doubles' `CreateProgressReport()` now surfaces the base's `StartedAt`/`Elapsed` timing
  (Abstractions 0.14.0) in the `Report`, so `ItemsPerSecond` is computed for reported progress;
  the extractor doubles also set `TotalItemCount` from a materialized collection source so
  `PercentComplete`/`EstimatedRemaining` compute.

### Fixed

- Surfacing `Report` timing from the doubles no longer throws `MissingMethodException` on
  **.NET 6 / .NET 7**. The doubles' `netstandard2.0` assembly (which those runtimes load) set the
  `Report` timing via the object-initializer (`init`) form, whose `IsExternalInit` modreq did not
  match the modern Abstractions assembly resolved at runtime. The doubles now use the plain
  `Report` timing constructor added in Abstractions 0.18.1, which is safe across every framework.

## [0.10.1] - 2026-07-24

Maintenance release: the deferred "thorough-review" hardening tier plus the
`Wolfgang.Etl.Abstractions` 0.17.0 bump. **No public API or behaviour change** to
either shipped package — the test doubles and contract-test base classes are
unchanged.

### Changed

- Built against `Wolfgang.Etl.Abstractions` 0.17.0 (was 0.15.0).

### Security

- Release now publishes via **OIDC / NuGet Trusted Publishing** (`NuGet/login`), removing
  the long-lived `NUGET_API_KEY` from the release path.
- Added supply-chain / security CI: transitive-dependency **license audit**, **CycloneDX SBOM**,
  **OSSF Scorecard**, **Semgrep** SAST, GitHub **Actions audit** (actionlint + zizmor, all
  actions SHA-pinned), and **build-reproducibility** verification with a per-release
  reproducible-build manifest attached to each GitHub Release.
- Documented the release path and compromise scope in `SECURITY.md`, and added a
  consumer-side reproducible-build verification guide (`docs/REPRODUCIBLE-BUILD.md`).

## [0.10.0] - 2026-06-29

Adds an opt-in contract-test base for the `ISupportDryRun` interface introduced in
`Wolfgang.Etl.Abstractions` 0.15.0. No breaking change.

### Added

- `SupportsDryRunContractTests<TSut>` — an opt-in xUnit contract-test base that verifies
  a stage implementing `ISupportDryRun` actually *skips its external side effect* when
  `IsDryRun` is `true` (not merely exposes the property). Stage-agnostic: the derived test
  supplies `CreateSut()` and a single `RunAndReportSideEffectAsync(bool)`. Requires
  `Wolfgang.Etl.Abstractions` 0.15.0. (ETL-Abstractions#259, ETL-Test-Kit#195)
- `TestLoader<T>` now implements `ISupportDryRun`: in dry-run mode it still enumerates the
  source and advances progress counters but skips collecting items, serving as the
  reference implementation exercised by the new contract base.

### Changed

- Built against `Wolfgang.Etl.Abstractions` 0.15.0 (was 0.14.1).

## [0.9.0] - 2026-06-23

First feature release of the test doubles and contract-test surface since 0.8.x. Adds fault-injection doubles, factory-based test data, progress-assertion helpers, and opt-in idempotency contract tests. Now built against `Wolfgang.Etl.Abstractions` 0.14.0, which brings per-run counter/timing resets and disposability to the base classes.

### Added
- `FaultyExtractor<T>`, `FaultyLoader<T>`, and `FaultyTransformer<T>` fault-injection doubles with `ThrowAt`, `ThrowAfterCompletion`, and `DuplicateAt` knobs for exercising error and retry paths.
- `TestExtractor<T>` factory constructors taking `Func<T>` or `Func<int, T>` (with an optional item count) to generate test data without materializing collections up front.
- `ProgressCapture<T>` and `ProgressAssert` — xUnit helpers for capturing and asserting on `IProgress<T>` reports.
- Opt-in `IdempotentExtractorContractTests`, `IdempotentLoaderContractTests`, and `IdempotentTransformerContractTests` base classes for verifying that a component produces identical results across repeated runs.
- A BenchmarkDotNet baseline project plus a gh-pages benchmark chart workflow.
- Additional contract tests covering argument `ParamName` validation, per-item count ordering, and no-over-read behavior (issues #46, #49, #50).

### Changed
- Now requires `Wolfgang.Etl.Abstractions` **0.14.0**, which resets per-run counters and timing at the start of each run (a behavioral change), adds timing/throughput metrics to `Report`, and makes the base classes `IDisposable`/`IAsyncDisposable`.
- The Stryker mutation-testing workflow now runs on Windows across all target frameworks.

## [0.8.1] - 2026-06-19

Canonical maintenance round. Library public API and runtime behavior are unchanged from 0.8.0 — this release ships the C4 binding-stability fix plus the canonical CI / docs / metadata work folded in from the stacked canonical PRs (canonical-protected, canonical-unprotected, protected/d8-verify-docs-build-fleet, fix/restore-assemblyversion, chore/remove-post-setup-bootstrap-files).

### Added
- **D8** verify-docs-build job in `release.yaml` (deduplicated).
- **A1** PublicApiAnalyzers scaffolding in `Directory.Build.props` (baseline file deferred to IDE-fix pass).
- **CI3** canonical NuGet metadata: `Authors`, `Copyright`, SourceLink, `.snupkg` symbol packages.
- **T3** Stryker mutation-testing workflow (`stryker.yaml`).
- **T1** coverage report published to the docs site.
- **S1** CodeQL `security-extended` query pack.
- **D6** `versions.json` preservation guard in `docfx.yaml`.
- **D7** docs build cache hygiene.
- **CI2** Dependabot `github-actions` ecosystem.
- `docs/DOCFX-VERSION-PICKER.md` (the D8 bulk fanout dropped this on some repos; added directly).

### Changed
- **C1** fleet template-drift sync.
- `<Nullable>enable</Nullable>` consolidated into `Directory.Build.props`.
- **D3** script hardening.
- Analyzer `PackageReference`s centralized in `Directory.Build.props`.

### Fixed
- **C4** restored explicit `<AssemblyVersion>1.0.0.0</AssemblyVersion>` + prerelease-safe `<FileVersion>` for .NET Framework binding stability.
- Duplicate `verify-docs-build:` job key in `release.yaml`.
- `.gitattributes` merge conflict resolved by keeping the canonical documented variant.

### Removed
- Post-setup bootstrap files (`scripts/setup.ps1`, `scripts/Setup-BranchRuleset.ps1`, `scripts/Setup-GitHubPages.ps1`) — template carry-overs no longer needed on this long-lived repo.
