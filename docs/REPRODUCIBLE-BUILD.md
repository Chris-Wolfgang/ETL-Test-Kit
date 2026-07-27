# Verifying the build is reproducible

Every release of `Wolfgang.Etl.TestKit` and `Wolfgang.Etl.TestKit.Xunit` is built
deterministically: the same source at the same tag produces byte-identical
assemblies, independent of who builds it or where. This page lets **you** confirm
that independently, so "our builds are reproducible" is a checkable claim rather
than a promise.

CI already proves *same-environment* reproducibility on every push
([`reproducible-build.yaml`](../.github/workflows/reproducible-build.yaml), #135):
it builds the library twice and asserts the assembly hashes match. This document
is the *consumer-side* flip — how a third party reproduces and attests to it.

## What is published

Each GitHub Release attaches a **`reproducible-build-manifest.json`** listing the
SHA-256 of every shipped assembly, plus the reference environment (OS and .NET SDK
version) and the exact build command used to produce them. Example:

```json
{
  "schema": "wolfgang.reproducible-build-manifest/v1",
  "version": "v0.11.0",
  "targetFramework": "net10.0",
  "buildCommand": "dotnet build <project> -c Release -f net10.0 -p:ContinuousIntegrationBuild=true",
  "referenceEnvironment": { "os": "Linux", "dotnetSdk": "10.0.110" },
  "assemblies": [
    { "assembly": "Wolfgang.Etl.TestKit.dll", "sha256": "…" },
    { "assembly": "Wolfgang.Etl.TestKit.Xunit.dll", "sha256": "…" }
  ]
}
```

## Reproduce it yourself

1. **Match the reference environment.** Use the same OS family and .NET SDK
   version named in the release's manifest (`referenceEnvironment`).
   `ContinuousIntegrationBuild=true` normalises source paths, so the *checkout
   location* does not matter — but the compiler version does, so match the SDK.

2. **Clone at the exact tag:**

   ```bash
   git clone --branch <tag> --depth 1 https://github.com/Chris-Wolfgang/ETL-Test-Kit
   cd ETL-Test-Kit
   ```

3. **Build each library with the documented command** (the manifest's
   `buildCommand`):

   ```bash
   dotnet build src/Wolfgang.Etl.TestKit/Wolfgang.Etl.TestKit.csproj \
     -c Release -f net10.0 -p:ContinuousIntegrationBuild=true
   dotnet build src/Wolfgang.Etl.TestKit.Xunit/Wolfgang.Etl.TestKit.Xunit.csproj \
     -c Release -f net10.0 -p:ContinuousIntegrationBuild=true
   ```

4. **Hash your output and compare** against the manifest:

   ```bash
   sha256sum \
     src/Wolfgang.Etl.TestKit/bin/Release/net10.0/Wolfgang.Etl.TestKit.dll \
     src/Wolfgang.Etl.TestKit.Xunit/bin/Release/net10.0/Wolfgang.Etl.TestKit.Xunit.dll
   ```

   Each hash must equal the corresponding `sha256` in
   `reproducible-build-manifest.json`. The repo's own generator
   ([`scripts/reproducible-manifest.sh`](../scripts/reproducible-manifest.sh))
   runs exactly these steps, so you can also regenerate the whole manifest and
   `diff` it against the published one.

## If a hash does not match

A mismatch means either the environments differ (most commonly a different SDK
patch version) or the artifact was tampered with. Please
[open an issue](https://github.com/Chris-Wolfgang/ETL-Test-Kit/issues/new) titled
"Reproducible-build mismatch for `<tag>`" including:

- the release tag,
- your OS and `dotnet --version`,
- your computed hashes vs the manifest's,
- the exact commands you ran.

## Publishing a third-party attestation

Independent verification is most useful when it is *public*. If you reproduced a
release successfully, you can publish an attestation following the
[Reproducible Builds project](https://reproducible-builds.org/) conventions (or a
service such as [vouchsafe.io](https://vouchsafe.io/)): sign a statement naming the
tag, the manifest hash, and your environment, and link it back on the mismatch/
verification issue so others can find corroborating rebuilds.
