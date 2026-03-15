using System.Linq;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestExtractorWithCancellationAsyncContractTests
    : ExtractWithCancellationAsyncContractTests<TestExtractor<int>, int>
{
    protected override TestExtractor<int> CreateSut(int itemCount) => new(Enumerable.Range(1, itemCount).ToList());
}
