using System.Linq;
using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestExtractorWithProgressAsyncContractTests
    : ExtractWithProgressAsyncContractTests<TestExtractor<int>, int, Report>
{
    protected override TestExtractor<int> CreateSut(int itemCount) => new(Enumerable.Range(1, itemCount).ToList());
}
