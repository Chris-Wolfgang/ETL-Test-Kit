namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestLoaderAsyncContractTests
    : LoadAsyncContractTests<TestLoader<int>, int>
{
    protected override TestLoader<int> CreateSut(int itemCount) => new(collectItems: false);
}
