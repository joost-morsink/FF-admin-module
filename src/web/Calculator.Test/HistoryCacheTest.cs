using System.Collections.Immutable;

namespace FfAdmin.Calculator.Test;

[TestClass]
public class HistoryCacheTest
{
    [TestMethod]
    public void SimpleTest()
    {
        var hc = new HistoryCache<string>(x => x.ToString());
        hc.GetAtPosition(1000).Should().Be("1000");
    }
}
