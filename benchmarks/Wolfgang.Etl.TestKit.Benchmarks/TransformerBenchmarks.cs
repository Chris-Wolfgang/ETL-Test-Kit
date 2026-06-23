using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Wolfgang.Etl.TestKit;

namespace Wolfgang.Etl.TestKit.Benchmarks;

/// <summary>
/// Benchmarks the primary <see cref="TestTransformer{T}"/> path: streaming an
/// <see cref="IAsyncEnumerable{T}"/> source through <c>TransformAsync</c> and
/// enumerating every yielded item.
/// </summary>
[MemoryDiagnoser]
public class TransformerBenchmarks
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
    public async Task<int> Transform()
    {
        var transformer = new TestTransformer<int>();

        var count = 0;
        await foreach (var _ in transformer.TransformAsync(ToAsyncEnumerable(_items)))
        {
            count++;
        }

        return count;
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
