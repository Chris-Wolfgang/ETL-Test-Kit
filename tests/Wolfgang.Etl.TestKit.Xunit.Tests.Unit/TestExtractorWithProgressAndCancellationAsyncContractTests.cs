using System.Collections.Generic;
using System.Linq;
using Wolfgang.Etl.Abstractions;
using Wolfgang.Etl.TestKit;
using Wolfgang.Etl.TestKit.Xunit;

namespace Wolfgang.Etl.TestKit.Xunit.Tests.Unit;

public class TestExtractorWithProgressAndCancellationAsyncContractTests
    : ExtractWithProgressAndCancellationAsyncContractTests<TestExtractor<int>, int, Report>
{
    protected override TestExtractor<int> CreateSut(int itemCount) =>
        new TestExtractor<int>(Enumerable.Range(1, itemCount).ToList());
}
