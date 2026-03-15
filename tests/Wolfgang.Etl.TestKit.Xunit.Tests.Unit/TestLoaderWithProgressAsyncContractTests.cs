using Wolfgang.Etl.Abstractions;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestLoaderWithProgressAsyncContractTests
    : LoadWithProgressAsyncContractTests<TestLoader<int>, int, Report>
{
    protected override TestLoader<int> CreateSut(int itemCount) => new(collectItems: false);
}
