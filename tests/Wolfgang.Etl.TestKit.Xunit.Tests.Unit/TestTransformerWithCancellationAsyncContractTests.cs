namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestTransformerWithCancellationAsyncContractTests
    : TransformWithCancellationAsyncContractTests<TestTransformer<int>, int>
{
    protected override TestTransformer<int> CreateSut(int itemCount) =>
        new TestTransformer<int>();
}
