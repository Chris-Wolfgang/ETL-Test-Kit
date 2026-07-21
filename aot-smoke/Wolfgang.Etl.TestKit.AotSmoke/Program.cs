// Native-AOT smoke consumer for Wolfgang.Etl.TestKit (#132).
//
// Exercises the public surface after native-AOT publish and returns a non-zero
// exit code if any invariant is violated, so the CI job fails if the library
// stops being AOT-safe at publish/run time. Kept deliberately simple — this is a
// smoke test, not a behavioural test suite (those live in the unit-test project).

using Wolfgang.Etl.TestKit;

static int Fail(string message)
{
    Console.Error.WriteLine($"AOT smoke FAILED: {message}");
    return 1;
}

// Extractor: yields the seeded items through the async-enumerable pipeline.
var extractor = new TestExtractor<int>(new[] { 1, 2, 3 });
var extracted = new List<int>();
await foreach (var item in extractor.ExtractAsync())
{
    extracted.Add(item);
}
if (extracted.Count != 3)
{
    return Fail($"TestExtractor yielded {extracted.Count} items, expected 3");
}

// Transformer: pass-through over the extracted items.
var transformer = new TestTransformer<int>();
var transformed = new List<int>();
await foreach (var item in transformer.TransformAsync(ToAsync(extracted)))
{
    transformed.Add(item);
}
if (transformed.Count != extracted.Count)
{
    return Fail($"TestTransformer yielded {transformed.Count} items, expected {extracted.Count}");
}

// Loader: collects everything the pipeline pushed.
var loader = new TestLoader<int>(collectItems: true);
await loader.LoadAsync(ToAsync(transformed));
var collected = loader.GetCollectedItems();
if (collected is null || collected.Count != transformed.Count)
{
    return Fail($"TestLoader collected {(collected is null ? "null" : collected.Count.ToString())} items, expected {transformed.Count}");
}

Console.WriteLine($"AOT smoke OK: extracted {extracted.Count}, transformed {transformed.Count}, loaded {collected.Count}");
return 0;

static async IAsyncEnumerable<int> ToAsync(IEnumerable<int> items)
{
    foreach (var item in items)
    {
        yield return item;
    }

    await Task.CompletedTask;
}
