using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.TestKit.Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestTransformerWithProgressAsyncContractTests
    : TransformWithProgressAsyncContractTests<TestTransformer<int>, int, Report>
{
    protected override TestTransformer<int> CreateSut(int itemCount) =>
        new TestTransformer<int>();
}
