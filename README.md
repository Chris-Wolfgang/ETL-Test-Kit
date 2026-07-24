# Wolfgang.Etl.TestKit

An Extractor, Transformer and Loader designed to be used in testing libraries built with Wolfgang.Etl.Abstractions

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Multi--Targeted-purple.svg)](https://dotnet.microsoft.com/)
[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/Chris-Wolfgang/ETL-Test-Kit)

---

## 📦 Installation

This repo ships two NuGet packages.

**Core** — test doubles for building ETL test fixtures:

```bash
dotnet add package Wolfgang.Etl.TestKit
```

**xUnit add-on** — abstract xUnit contract-test base classes for verifying custom extractors, transformers, and loaders built on `Wolfgang.Etl.Abstractions`:

```bash
dotnet add package Wolfgang.Etl.TestKit.Xunit
```

**NuGet packages:**

- [Wolfgang.Etl.TestKit](https://www.nuget.org/packages/Wolfgang.Etl.TestKit) — test doubles
- [Wolfgang.Etl.TestKit.Xunit](https://www.nuget.org/packages/Wolfgang.Etl.TestKit.Xunit) — xUnit contract tests

Install `Wolfgang.Etl.TestKit.Xunit` whenever you author a custom `ExtractorBase` / `LoaderBase` / `TransformerBase` and want your test project to inherit the canonical xUnit contract coverage with zero boilerplate:

```csharp
public sealed class MyExtractorTests : ExtractorBaseContractTests<MyExtractor, MyItem, MyProgress>
{
    protected override MyExtractor CreateSut(int itemCount) => new MyExtractor(itemCount);
    protected override IReadOnlyList<MyItem> CreateExpectedItems() => ...;
    protected override MyExtractor CreateSutWithTimer(IProgressTimer timer) => ...;
}
```
---

## 📄 License

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.

---

## 📚 Documentation

- **GitHub Repository:** [https://github.com/Chris-Wolfgang/ETL-Test-Kit](https://github.com/Chris-Wolfgang/ETL-Test-Kit)
- **API Documentation:** https://Chris-Wolfgang.github.io/ETL-Test-Kit/
- **Formatting Guide:** [docs/README-FORMATTING.md](docs/README-FORMATTING.md)
- **Contributing Guide:** [CONTRIBUTING.md](CONTRIBUTING.md)

---

## 🚀 Quick Start

The kit ships two packages that solve different problems. Use the **core** test doubles to drive a pipeline in a test or benchmark; use the **xUnit add-on** to verify your own `ExtractorBase` / `LoaderBase` / `TransformerBase` implementations against the full behavioural contract.

### Core — driving a pipeline with the test doubles

`TestExtractor<T>`, `TestTransformer<T>`, and `TestLoader<T>` let you wire up an extract → transform → load pipeline entirely in memory. All three are generic over the item type, which must be `notnull`.

```csharp
using Wolfgang.Etl.TestKit;

var source = new[] { "alpha", "bravo", "charlie" };

var extractor   = new TestExtractor<string>(source);
var transformer = new TestTransformer<string>();          // pass-through, returns each item unchanged
var loader      = new TestLoader<string>(collectItems: true);

await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));

IReadOnlyList<string>? loaded = loader.GetCollectedItems();
// loaded => [ "alpha", "bravo", "charlie" ]
```

`TestExtractor<T>` also accepts an `IEnumerator<T>` so you can stream large, generated sequences without materializing them:

```csharp
static IEnumerator<int> Generate(int count)
{
    for (var i = 0; i < count; i++)
        yield return i;
}

var extractor = new TestExtractor<int>(Generate(1_000_000));
```

Construct `TestLoader<T>` with `collectItems: false` to enumerate the full pipeline (for realistic benchmark throughput) without storing items — `GetCollectedItems()` returns `null` in that mode.

### Core — generating data with `TestExtractor<T>` factory constructors

Instead of materializing a collection, build a `TestExtractor<T>` from a factory delegate. Pass a `Func<T>` (or a `Func<int, T>` that receives the zero-based index) plus an optional item count:

```csharp
using Wolfgang.Etl.TestKit;

// Func<int, T> — receives the index; here capped at 1,000 items
var indexed = new TestExtractor<string>(i => $"row-{i}", count: 1_000);

// Func<T> — same value each call; capped at 5 items
var constant = new TestExtractor<string>(() => "ping", count: 5);
```

### Core — injecting failures with the `Faulty*` doubles

`FaultyExtractor<T>`, `FaultyLoader<T>`, and `FaultyTransformer<T>` let you exercise error and retry paths. The `ThrowAt`, `ThrowAfterCompletion`, and `DuplicateAt` methods are fluent and chainable:

```csharp
using System;
using Wolfgang.Etl.TestKit;

var source = new[] { "alpha", "bravo", "charlie", "delta" };

var extractor = new FaultyExtractor<string>(source)
    .ThrowAt(2, new InvalidOperationException("boom on the third item"));

// Or fail only after all items are produced:
var afterAll = new FaultyExtractor<string>(source)
    .ThrowAfterCompletion(new TimeoutException());

// Or emit a duplicate of the item at a given index (to test de-duplication):
var loader = new FaultyLoader<string>(collectItems: true)
    .DuplicateAt(1);
```

### xUnit — capturing and asserting on progress

`ProgressCapture<T>` is an `IProgress<T>` that records every report; pass it straight to any progress-aware overload, then assert with `ProgressAssert`:

```csharp
using System.Threading;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.TestKit.Xunit;

var capture = new ProgressCapture<Report>();
var extractor = new TestExtractor<string>(new[] { "a", "b", "c" });

await foreach (var _ in extractor.ExtractAsync(capture, CancellationToken.None)) { }

ProgressAssert.HasReports(capture);
ProgressAssert.IsMonotonicallyIncreasing(capture, r => r.CurrentItemCount);

Report? last = capture.FinalReport;   // the final report, or null if none captured
```

### xUnit — verifying idempotency

When a component must produce identical results across repeated runs, derive from the matching opt-in idempotency base and implement its factory methods:

```csharp
using System.Collections.Generic;
using Wolfgang.Etl.TestKit.Xunit;

public sealed class MyExtractorIdempotencyTests
    : IdempotentExtractorContractTests<MyExtractor, MyRecord, MyProgress>
{
    protected override MyExtractor CreateSut(int itemCount) => new MyExtractor(itemCount);

    protected override IReadOnlyList<MyRecord> CreateExpectedItems() =>
        new List<MyRecord> { new("a"), new("b"), new("c"), new("d"), new("e") };
}
```

`IdempotentLoaderContractTests<TSut, TItem>` adds a `TryGetLoadedItems(TSut sut)` factory (return `null` if the loader does not expose its loaded items), and `IdempotentTransformerContractTests<TSut, TItem>` follows the extractor shape with `CreateExpectedItems()`.

### xUnit add-on — contract-testing your own ETL types

Derive your test class from the matching contract base and implement the abstract factory methods. You inherit the complete suite of `ExtractAsync` / `TransformAsync` / `LoadAsync` contract tests — all overloads, cancellation, progress, `SkipItemCount`, and `MaximumItemCount` — with zero boilerplate.

```csharp
using System.Collections.Generic;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit.Xunit;

public sealed class MyExtractorContractTests
    : ExtractorBaseContractTests<MyExtractor, MyRecord, MyProgress>
{
    protected override MyExtractor CreateSut(int itemCount) =>
        new MyExtractor("path/to/test-data.csv", itemCount);

    protected override IReadOnlyList<MyRecord> CreateExpectedItems() =>
        new List<MyRecord> { new("a"), new("b"), new("c"), new("d"), new("e") };

    protected override MyExtractor CreateSutWithTimer(IProgressTimer timer) =>
        new MyExtractor("path/to/test-data.csv", timer);
}
```

`LoaderBaseContractTests<TSut, TItem, TProgress>` follows the same shape, except its source factory is named `CreateSourceItems()`:

```csharp
public sealed class MyLoaderContractTests
    : LoaderBaseContractTests<MyLoader, MyRecord, MyProgress>
{
    protected override MyLoader CreateSut(int itemCount) => new MyLoader(connectionString);

    protected override IReadOnlyList<MyRecord> CreateSourceItems() =>
        new List<MyRecord> { new("a"), new("b"), new("c"), new("d"), new("e") };

    protected override MyLoader CreateSutWithTimer(IProgressTimer timer) =>
        new MyLoader(connectionString, timer);
}
```

`TransformerBaseContractTests<TSut, TItem, TProgress>` uses `CreateExpectedItems()` (like the extractor) and constrains `TSut` to `TransformerBase<TItem, TItem, TProgress>`.

> **Note:** Expose a `protected` constructor that accepts an `IProgressTimer` on your extractor/loader/transformer so `CreateSutWithTimer` can inject a `ManualProgressTimer` to fire progress callbacks on demand. `CreateExpectedItems()` / `CreateSourceItems()` must return at least 5 items.

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| **`TestExtractor<T>`** | In-memory extractor that yields items from an `IEnumerable<T>` (reusable across runs) or an `IEnumerator<T>` (for large, on-the-fly generated sequences) |
| **`TestTransformer<T>`** | Pass-through transformer that returns each item unchanged — useful when a pipeline needs a transformer stage but the test focuses on the extractor or loader |
| **`TestLoader<T>`** | In-memory loader that always enumerates the full stream; with `collectItems: true` it buffers items for assertion via `GetCollectedItems()`, with `false` it measures throughput without storing |
| **Pagination** | `SkipItemCount` and `MaximumItemCount` on every test double for skipping and limiting items |
| **Progress reporting** | Timer-based `IProgress<Report>` callbacks, with a `protected` `IProgressTimer` constructor for deterministic, on-demand firing in tests |
| **Contract test bases** | `ExtractorBaseContractTests<,,>`, `LoaderBaseContractTests<,,>`, and `TransformerBaseContractTests<,,>` — comprehensive xUnit suites covering every `Wolfgang.Etl.Abstractions` base-class behaviour |
| **`ManualProgressTimer`** | An `IProgressTimer` whose `Fire()` method triggers progress callbacks synchronously, so progress tests are deterministic |
| **`SynchronousProgress<T>`** | An `IProgress<T>` that invokes its callback synchronously for predictable progress assertions |
| **`TestExtractor<T>` factory ctors** | Build a `TestExtractor<T>` from a `Func<T>` or `Func<int, T>` factory (with an optional item count) instead of materializing a collection up front |
| **`FaultyExtractor<T>` / `FaultyLoader<T>` / `FaultyTransformer<T>`** | Fault-injection doubles with fluent `ThrowAt`, `ThrowAfterCompletion`, and `DuplicateAt` knobs for exercising error and retry paths |
| **`ProgressCapture<T>` + `ProgressAssert`** | `ProgressCapture<T>` is an `IProgress<T>` that records every report; `ProgressAssert` provides xUnit assertions (`HasReports`, `HasExactly`, `FinalReportSatisfies`, `IsMonotonicallyIncreasing`, …) over a capture |
| **`Idempotent*ContractTests`** | Opt-in `IdempotentExtractorContractTests<,,>`, `IdempotentLoaderContractTests<,>`, and `IdempotentTransformerContractTests<,>` bases that verify a component produces identical results across repeated runs |
| **Multi-TFM support** | net462, net481, netstandard2.0, net8.0, net10.0 |

---

## 🎯 Supported Frameworks

This library targets:

- **.NET Framework:** 4.6.2

See the [NuGet package page](https://www.nuget.org/packages/Wolfgang.Etl.TestKit/) for the authoritative per-TFM compatibility matrix.

## 🔍 Code Quality & Static Analysis

This project enforces **strict code quality standards** through **7 specialized analyzers** and custom async-first rules:

### Analyzers in Use

1. **Microsoft.CodeAnalysis.NetAnalyzers** - Built-in .NET analyzers for correctness and performance
2. **Roslynator.Analyzers** - Advanced refactoring and code quality rules
3. **AsyncFixer** - Async/await best practices and anti-pattern detection
4. **Microsoft.VisualStudio.Threading.Analyzers** - Thread safety and async patterns
5. **Microsoft.CodeAnalysis.BannedApiAnalyzers** - Prevents usage of banned synchronous APIs
6. **Meziantou.Analyzer** - Comprehensive code quality rules
7. **SonarAnalyzer.CSharp** - Industry-standard code analysis

### Async-First Enforcement

This library uses **`BannedSymbols.txt`** to prohibit synchronous APIs and enforce async-first patterns:

**Blocked APIs Include:**
- ❌ `Task.Wait()`, `Task.Result` - Use `await` instead
- ❌ `Thread.Sleep()` - Use `await Task.Delay()` instead
- ❌ Synchronous file I/O (`File.ReadAllText`) - Use async versions
- ❌ Synchronous stream operations - Use `ReadAsync()`, `WriteAsync()`
- ❌ `Parallel.For/ForEach` - Use `Task.WhenAll()` or `Parallel.ForEachAsync()`
- ❌ Obsolete APIs (`WebClient`, `BinaryFormatter`)

**Why?** To ensure all code is **truly async** and **non-blocking** for optimal performance in async contexts.

---

## 🛠️ Building from Source

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Optional: [PowerShell Core](https://github.com/PowerShell/PowerShell) for formatting scripts

### Build Steps

```bash
# Clone the repository
git clone https://github.com/Chris-Wolfgang/ETL-Test-Kit.git
cd ETL-Test-Kit

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release

# Run code formatting (PowerShell Core)
pwsh ./scripts/format.ps1
```

### Code Formatting

This project uses `.editorconfig` and `dotnet format`:

```bash
# Format code
dotnet format

# Verify formatting (as CI does)
dotnet format --verify-no-changes
```

See [docs/README-FORMATTING.md](docs/README-FORMATTING.md) for detailed formatting guidelines.

### Building Documentation

This project uses [DocFX](https://dotnet.github.io/docfx/) to generate API documentation:

```bash
# Install DocFX (one-time setup)
dotnet tool install -g docfx

# Generate API metadata and build documentation
cd docfx_project
docfx metadata  # Extract API metadata from source code
docfx build     # Build HTML documentation

# Documentation is generated in the docs/ folder at the repository root
```

The documentation is automatically built and deployed to GitHub Pages when changes are pushed to the `main` branch.

**Local Preview:**
```bash
# Serve documentation locally (with live reload)
cd docfx_project
docfx build --serve

# Open http://localhost:8080 in your browser
```

**Documentation Structure:**
- `docfx_project/` - DocFX configuration and source files
- `docs/` - Generated HTML documentation (published to GitHub Pages)
- `docfx_project/index.md` - Main landing page content
- `docfx_project/docs/` - Additional documentation articles
- `docfx_project/api/` - Auto-generated API reference YAML files

---

## 🤝 Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for:
- Code quality standards
- Build and test instructions
- Pull request guidelines
- Analyzer configuration details

---


## 🙏 Acknowledgments

- **[Wolfgang.Etl.Abstractions](https://github.com/Chris-Wolfgang/ETL-Abstractions)** — provides the `ExtractorBase`, `LoaderBase`, and `TransformerBase` base classes, progress-reporting infrastructure, and the `IProgressTimer` contract that this test kit builds on and verifies.
- **[xUnit](https://xunit.net/)** — the test framework the `Wolfgang.Etl.TestKit.Xunit` contract base classes are built on.

