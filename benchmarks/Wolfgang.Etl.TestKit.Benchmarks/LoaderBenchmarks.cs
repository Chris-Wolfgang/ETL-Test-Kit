using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Wolfgang.Etl.TestKit;

namespace Wolfgang.Etl.TestKit.Benchmarks;

/// <summary>
/// Benchmarks the primary <see cref="TestLoader{T}"/> path: consuming an
/// <see cref="IAsyncEnumerable{T}"/> source via <c>LoadAsync</c> without collecting
/// items, so the measurement is dominated by the framework pipeline.
/// </summary>
[MemoryDiagnoser]
public class LoaderBenchmarks
{
    private int[] _items = System.Array.Empty<int>();



    [Params(1_000, 10_000, 100_000)]
    public int ItemCount { get; set; }



    [GlobalSetup]
    public void Setup()
    {
        _items = new int[ItemCount];
        for (var i = 0; i < ItemCount; i++)
        {
            _items[i] = i;
        }
    }



    [Benchmark(Baseline = true)]
    public async Task Load()
    {
        var loader = new TestLoader<int>(collectItems: false);

        await loader.LoadAsync(ToAsyncEnumerable(_items));
    }



    private static async IAsyncEnumerable<int> ToAsyncEnumerable
    (
        int[] items,
        [EnumeratorCancellation] System.Threading.CancellationToken token = default
    )
    {
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            yield return item;
        }

        await Task.CompletedTask;
    }
}
