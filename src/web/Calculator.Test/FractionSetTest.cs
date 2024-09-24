namespace FfAdmin.Calculator.Test;

[TestClass]
public class FractionSetTest : VerifyBase
{
    [TestMethod]
    public void SimpleTest()
    {
        var fs = FractionSet.Empty;
        fs=fs.Add("1", 5);
        fs=fs.Add("2", 1);
        fs=fs.AddRange(new [] { ("3",0.5m), ("4",0.5m) });
        fs.Divisor.Should().Be(4);
        fs["0"].Should().Be(0);
        fs["1"].Should().Be(0.25m);
    }

    [TestMethod]
    public void AnotherTest()
    {
        var fs = FractionSet.Empty
            .AddRange(new[] {("1", 2m), ("2", 1m), ("3", 1m)})
            .Add("4", 0.25m);
        fs.Divisor.Should().Be(5);
        fs["1"].Should().Be(0.4m);
        fs["2"].Should().Be(0.2m);
        fs["3"].Should().Be(0.2m);
        fs["4"].Should().Be(0.2m);
    }

    [TestMethod]
    public void RandomTest()
    {
        var rnd = new Random();
        var fs = Enumerable.Range(0, 100).Select((x, i) => (i, rnd.NextDouble()))
            .Aggregate(FractionSet.Empty, (acc, f) => acc.Add(f.i.ToString(), (decimal)f.Item2));
        fs.Values.Sum().Should().BeApproximately(1m, 0.000000000001m);
    }

    [TestMethod]
    public void AggregateTest()
    {
        var fs = FractionSet.Empty
            .AddRange(new[] {("1", 2m), ("2", 1m), ("3", 1m), ("4", 1m)});
        var agg = fs.Aggregate(x => (int.Parse(x) % 2).ToString());
        agg.Divisor.Should().Be(5);
        agg["0"].Should().Be(0.4m);
        agg["1"].Should().Be(0.6m);
    }
    [TestMethod]
    public void GroupTest()
    {
        var fs = FractionSet.Empty
            .AddRange(new[] {("1", 2m), ("2", 1m), ("3", 1m), ("4", 1m)});
        var agg = fs.Group(x => (int.Parse(x) % 2).ToString());
        agg["0"]["2"].Should().Be(0.5m);
        agg["0"]["4"].Should().Be(0.5m);
        agg["1"]["1"].Should().Be(2m/3m);
        agg["1"]["3"].Should().Be(1m/3m);
    }

    [TestMethod]
    public void SerializationTest()
    {
        var fs = FractionSet.Empty
            .AddRange(new[] {("1", 2m), ("2", 1m), ("3", 1m), ("4", 1m)})
            .Add("5",0.6m);
        
        var json = System.Text.Json.JsonSerializer.Serialize(fs);
        fs.Divisor.Should().Be(8);
        json.Should().Contain($"\"Divisor\":{Math.Round(fs.Divisor)}");
        foreach(var item in fs)
            json.Should().Contain($"\"{item.Key}\":{Math.Round(item.Value * fs.Divisor)}");
    }
}
