namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestTransformerAsyncContractTests
    : TransformAsyncContractTests<TestTransformer<int>, int>
{
    protected override TestTransformer<int> CreateSut(int itemCount) => new();
}
