using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestTransformerWithProgressAsyncContractTests
    : TransformWithProgressAsyncContractTests<TestTransformer<int>, int, Report>
{
    protected override TestTransformer<int> CreateSut(int itemCount) =>
        new TestTransformer<int>();
}
