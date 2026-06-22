# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed

### Deprecated

### Removed

### Fixed

### Security

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
