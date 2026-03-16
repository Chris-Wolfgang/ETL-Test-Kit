# Verifying ETL Components with Wolfgang.Etl.TestKit.Xunit

The `Wolfgang.Etl.TestKit.Xunit` package provides a suite of abstract xUnit base classes
that verify any ETL component built on `Wolfgang.Etl.Abstractions` satisfies its
contractual obligations.

Inherit from the appropriate base class in your own test project, implement the factory
methods, and you instantly gain a comprehensive test suite covering extraction,
transformation, and loading behaviour, cancellation, progress reporting, and property
validation — with no test code to write yourself.

---

## Installation

Add `Wolfgang.Etl.TestKit.Xunit` to your **test** project.

**.NET CLI:**

```bash
dotnet add package Wolfgang.Etl.TestKit.Xunit
```

**Package Manager Console:**

```powershell
Install-Package Wolfgang.Etl.TestKit.Xunit
```

The package requires xUnit 2.7 or later and targets `net462`, `net481`,
`netstandard2.0`, `net8.0`, and `net10.0`.

---

## Choosing the right base class

There are two tiers of base class:

### Interface contract classes (thin)

Use these when your type implements one of the ETL interfaces directly but does **not**
inherit from an `ExtractorBase`, `TransformerBase`, or `LoaderBase`. They test only the
obligations declared by that interface.

| Base class | Interface verified |
|---|---|
| `ExtractAsyncContractTests<TSut, TItem>` | `IExtractAsync<TSource>` |
| `ExtractWithCancellationAsyncContractTests<TSut, TItem>` | `IExtractWithCancellationAsync<TSource>` |
| `ExtractWithProgressAsyncContractTests<TSut, TItem, TProgress>` | `IExtractWithProgressAsync<TSource, TProgress>` |
| `ExtractWithProgressAndCancellationAsyncContractTests<TSut, TItem, TProgress>` | `IExtractWithProgressAndCancellationAsync<TSource, TProgress>` |

### Base class contract classes (comprehensive)

Use these when your type inherits from one of the abstract base classes. They cover all
public overloads and every property defined by the base class.

| Base class | What is tested |
|---|---|
| `ExtractorBaseContractTests<TSut, TItem, TProgress>` | All `ExtractorBase` behaviour |
| `TransformerBaseContractTests<TSut, TItem, TProgress>` | All `TransformerBase` behaviour |
| `LoaderBaseContractTests<TSut, TItem, TProgress>` | All `LoaderBase` behaviour |

---

## Extractor example

```csharp
public class MyExtractorContractTests
    : ExtractorBaseContractTests<MyExtractor, MyRecord, MyProgress>
{
    protected override MyExtractor CreateSut(int itemCount) =>
        new MyExtractor("path/to/test-data.csv", itemCount);

    protected override IReadOnlyList<MyRecord> CreateExpectedItems() =>
        new List<MyRecord>
        {
            new("Alice", 30),
            new("Bob",   25),
            new("Carol", 35),
            new("Dave",  28),
            new("Eve",   42),
        };

    protected override MyExtractor CreateSutWithTimer(IProgressTimer timer) =>
        new MyExtractor("path/to/test-data.csv", timer);
}
```

That single class exercises:

- All four `ExtractAsync` overloads return the expected items in order.
- `CurrentItemCount` increments as items are yielded.
- `ReportingInterval` rejects values less than 1.
- `MaximumItemCount` stops extraction at the specified limit and rejects values less than 0.
- `SkipItemCount` skips the specified number of items and rejects values less than 0.
- Progress callbacks fire when the timer fires.
- Cancellation is honoured on all cancellable overloads.

---

## Transformer example

```csharp
public class MyTransformerContractTests
    : TransformerBaseContractTests<MyTransformer, MyRecord, MyProgress>
{
    protected override MyTransformer CreateSut(int itemCount) =>
        new MyTransformer();

    protected override IReadOnlyList<MyRecord> CreateExpectedItems() =>
        new List<MyRecord>
        {
            new("Alice", 30),
            new("Bob",   25),
            new("Carol", 35),
            new("Dave",  28),
            new("Eve",   42),
        };

    protected override MyTransformer CreateSutWithTimer(IProgressTimer timer) =>
        new MyTransformer(timer);
}
```

That single class exercises:

- All four `TransformAsync` overloads return the expected items in order.
- `CurrentItemCount` increments as items are transformed.
- `ReportingInterval` rejects values less than 1.
- `MaximumItemCount` stops transformation at the specified limit and rejects values less than 1.
- `SkipItemCount` skips the specified number of items and rejects values less than 0.
- Progress callbacks fire when the timer fires.
- Cancellation is honoured on all cancellable overloads.

---

## Loader example

```csharp
public class MyLoaderContractTests
    : LoaderBaseContractTests<MyLoader, MyRecord, MyProgress>
{
    protected override MyLoader CreateSut(int itemCount) =>
        new MyLoader(connectionString);

    protected override IReadOnlyList<MyRecord> CreateSourceItems() =>
        new List<MyRecord>
        {
            new("Alice", 30),
            new("Bob",   25),
            new("Carol", 35),
            new("Dave",  28),
            new("Eve",   42),
        };

    protected override MyLoader CreateSutWithTimer(IProgressTimer timer) =>
        new MyLoader(connectionString, timer);
}
```

That single class exercises:

- All four `LoadAsync` overloads complete successfully.
- `CurrentItemCount` increments as items are loaded.
- `ReportingInterval` rejects values less than 1.
- `MaximumItemCount` stops loading at the specified limit and rejects values less than 1.
- `SkipItemCount` skips the specified number of items and rejects values less than 0.
- Progress callbacks fire when the timer fires.
- Cancellation is honoured on all cancellable overloads.

---

## MaximumItemCount and SkipItemCount requirements

The `MaximumItemCount` and `SkipItemCount` contract tests will only pass if your worker
method implementation checks these properties and acts accordingly. The base class sets
the properties but cannot enforce them on your behalf — that is intentional, because the
mechanism for stopping or skipping is specific to your data source.

A typical extractor implementation looks like this:

```csharp
protected override async IAsyncEnumerable<MyRecord> ExtractWorkerAsync(
    [EnumeratorCancellation] CancellationToken token)
{
    await foreach (var row in _source.ReadAllAsync(token).ConfigureAwait(false))
    {
        token.ThrowIfCancellationRequested();

        if (CurrentSkippedItemCount < SkipItemCount)
        {
            IncrementCurrentSkippedItemCount();
            continue;
        }

        if (CurrentItemCount >= MaximumItemCount)
            yield break;

        IncrementCurrentItemCount();
        yield return MapToRecord(row);
    }
}
```

---

## SynchronousProgress\<T\>

The package also ships `SynchronousProgress<T>`, a drop-in replacement for
`Progress<T>` that invokes its callback on the calling thread rather than posting to a
synchronisation context. This makes progress assertions in tests deterministic without
requiring delays or manual synchronisation.

```csharp
TProgress? lastReport = null;
var progress = new SynchronousProgress<TProgress>(r => lastReport = r);

await sut.ExtractAsync(progress).ToListAsync();

Assert.NotNull(lastReport);
Assert.Equal(expectedCount, lastReport.CurrentItemCount);
```

---

## Interface-only example

If your extractor implements `IExtractAsync<T>` directly rather than inheriting from
`ExtractorBase`, use the thin interface contract class instead:

```csharp
public class MyLightweightExtractorContractTests
    : ExtractAsyncContractTests<MyLightweightExtractor, MyRecord>
{
    protected override MyLightweightExtractor CreateSut(int itemCount) =>
        new MyLightweightExtractor(itemCount);

    protected override IReadOnlyList<MyRecord> CreateExpectedItems() =>
        new List<MyRecord> { new("a"), new("b"), new("c"), new("d"), new("e") };
}
```
