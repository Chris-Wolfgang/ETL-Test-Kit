using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Wolfgang.Etl.TestKit;

namespace Wolfgang.Etl.TestKit.Benchmarks;

/// <summary>
/// Benchmarks the primary <see cref="TestExtractor{T}"/> path: enumerating every
/// item out of an in-memory source via <c>ExtractAsync</c>.
/// </summary>
[MemoryDiagnoser]
public class ExtractorBenchmarks
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
    public async Task<int> Extract()
    {
        var extractor = new TestExtractor<int>(_items);

        var count = 0;
        await foreach (var _ in extractor.ExtractAsync())
        {
            count++;
        }

        return count;
    }
}
