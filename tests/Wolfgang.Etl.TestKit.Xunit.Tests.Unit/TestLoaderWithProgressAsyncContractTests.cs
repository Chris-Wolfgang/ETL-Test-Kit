using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.TestKit.Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestLoaderWithProgressAsyncContractTests
    : LoadWithProgressAsyncContractTests<TestLoader<int>, int, Report>
{
    protected override TestLoader<int> CreateSut(int itemCount) =>
        new TestLoader<int>(collectItems: false);
}
