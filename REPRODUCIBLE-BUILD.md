# Reproducible builds

Both shipped assemblies — `Wolfgang.Etl.TestKit` and `Wolfgang.Etl.TestKit.Xunit`
— are built to be **byte-for-byte reproducible**: the same source commit produces
the same compiled output regardless of *where* it is built.

## What makes the build reproducible

`Directory.Build.props` sets the compiler inputs that a reproducible build
requires:

- `<Deterministic>true</Deterministic>` — the compiler emits deterministic
  output (no embedded timestamps, ordered metadata).
- `<ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>` (in CI) —
  normalises embedded source paths to a deterministic `/_/` root via `PathMap`,
  so the checkout directory does not leak into the assembly.
- SourceLink — embeds the commit SHA rather than machine-local paths.

## How it is verified

[`.github/workflows/reproducible-build.yaml`](.github/workflows/reproducible-build.yaml)
checks the same commit out to two independent directories, builds each with
`-p:ContinuousIntegrationBuild=true`, and fails if the produced `.dll`s do not
hash identically (`sha256sum`). This proves **path-independent** reproducibility
on a single runner — the property that lets a third party rebuild and match.

## How to verify it yourself

```bash
git clone https://github.com/Chris-Wolfgang/ETL-Test-Kit a
git clone https://github.com/Chris-Wolfgang/ETL-Test-Kit b
for d in a b; do
  dotnet build "$d/src/Wolfgang.Etl.TestKit/Wolfgang.Etl.TestKit.csproj" \
    -c Release -f net10.0 -p:ContinuousIntegrationBuild=true
done
sha256sum \
  a/src/Wolfgang.Etl.TestKit/bin/Release/net10.0/Wolfgang.Etl.TestKit.dll \
  b/src/Wolfgang.Etl.TestKit/bin/Release/net10.0/Wolfgang.Etl.TestKit.dll
# The two hashes must be identical.
```

## Scope / follow-up

The verification above covers path-independent reproducibility on a single OS —
the fleet-proven guarantee. **Cross-OS** byte-identity (building on Ubuntu vs
Windows and matching) is a stronger claim that is not yet asserted here: `.pdb`
and some embedded metadata can differ across SDK patch levels and operating
systems even with deterministic inputs. Extending the matrix to cross-OS
comparison (with any required `.pdb`/metadata normalisation) is tracked as a
follow-up to #135.
