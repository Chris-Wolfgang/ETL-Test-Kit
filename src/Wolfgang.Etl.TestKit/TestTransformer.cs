using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit;

/// <summary>
/// A pass-through transformer for use in tests, examples, and benchmarks.
/// Returns each item unchanged, with no transformation applied.
/// </summary>
/// <typeparam name="T">The type of item to transform.</typeparam>
/// <remarks>
/// Useful when a pipeline requires a transformer in the chain but the test or
/// benchmark is focused on the extractor or loader in isolation.
/// </remarks>
/// <example>
/// <code>
/// var extractor   = new TestExtractor&lt;MyRecord&gt;(items);
/// var transformer = new TestTransformer&lt;MyRecord&gt;();
/// var loader      = new TestLoader&lt;MyRecord&gt;(collectItems: true);
///
/// await loader.LoadAsync(transformer.TransformAsync(extractor.ExtractAsync()));
/// </code>
/// </example>
public class TestTransformer<T> : TransformerBase<T, T, Report>
{
    // ------------------------------------------------------------------
    // TransformerBase overrides
    // ------------------------------------------------------------------

    /// <inheritdoc/>
    protected override Report CreateProgressReport() =>
        new Report(CurrentItemCount);



    /// <inheritdoc/>
#pragma warning disable CS1998 // async method with no await — required by IAsyncEnumerable contract
    protected override async IAsyncEnumerable<T> TransformWorkerAsync(
        IAsyncEnumerable<T> items,
        [EnumeratorCancellation] CancellationToken token)
#pragma warning restore CS1998
    {
        await foreach (var item in items.WithCancellation(token).ConfigureAwait(false))
        {
            IncrementCurrentItemCount();
            yield return item;
        }
    }
}
