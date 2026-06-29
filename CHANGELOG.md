# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `SupportsDryRunContractTests<TSut>` — an opt-in xUnit contract-test base that verifies
  a stage implementing `ISupportDryRun` actually *skips its external side effect* when
  `IsDryRun` is `true` (not merely exposes the property). Stage-agnostic: the derived test
  supplies `RunAsync` and `SideEffectOccurredAsync`. Requires `Wolfgang.Etl.Abstractions`
  0.15.0. (ETL-Abstractions#259, ETL-Test-Kit#195)
- `TestLoader<T>` now implements `ISupportDryRun`: in dry-run mode it still enumerates the
  source and advances progress counters but skips collecting items, serving as the
  reference implementation exercised by the new contract base.

### Changed

- Built against `Wolfgang.Etl.Abstractions` 0.15.0 (was 0.14.1).

### Deprecated

### Removed

### Fixed

### Security

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
