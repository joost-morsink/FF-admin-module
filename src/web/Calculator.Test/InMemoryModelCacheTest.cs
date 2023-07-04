namespace FfAdmin.Calculator.Test;

[TestClass]
public class InMemoryModelCacheTest
{
    private InMemoryModelCache<string> _sut = null!;

    [TestInitialize]
    public void Init()
    {
        _sut = new();
    }
    
    [TestMethod]
    public async Task GetAtPosition_WhenNoValue_ReturnsNull()
    {
        var res = await _sut.GetAtPosition(0); 
        res.Should().BeNull();
    }
    [TestMethod]
    public async Task GetAtPosition_WhenValue_ReturnsValue()
    {
        await _sut.SetAtPosition(0, "Hello");
        var res = await _sut.GetAtPosition(0);
        res.Should().Be("Hello");
    }
    [TestMethod]
    public async Task GetAtPosition_WhenValue_ReturnsValue2()
    {
        await _sut.SetAtPosition(0, "Hello");
        await _sut.SetAtPosition(1, "World");
        var res = await _sut.GetAtPosition(0);
        res.Should().Be("Hello");
        res = await _sut.GetAtPosition(1);
        res.Should().Be("World");
    } 
    [TestMethod]
    public async Task GetBaseIndex_ShouldFindNearestLower()
    {
        await _sut.SetAtPosition(0, "Hello");
        await _sut.SetAtPosition(4, "World");
        await _sut.SetAtPosition(8, "!");
        
        var res =await _sut.GetBasePosition(0);
        res.Should().Be(0);
        res =await _sut.GetBasePosition(1);
        res.Should().Be(0);
        res =await _sut.GetBasePosition(2);
        res.Should().Be(0);
        res =await _sut.GetBasePosition(3);
        res.Should().Be(0);
        res =await _sut.GetBasePosition(4);
        res.Should().Be(4);
        res =await _sut.GetBasePosition(5);
        res.Should().Be(4);
        res = await _sut.GetBasePosition(6);
        res.Should().Be(4);
        res = await _sut.GetBasePosition(7);
        res.Should().Be(4);
        res = await _sut.GetBasePosition(8);
        res.Should().Be(8);
        res = await _sut.GetBasePosition(9);
        res.Should().Be(8);
    }
}

[TestClass]
public class ModelCacheFactoryTest
{
    [TestMethod]
    public void CreateUntyped_ShouldNotThrowAndNotReturnNull()
    {
        IModelCacheFactory sut = new ModelCacheFactory();
        new Action(() => sut.Create(typeof(string), "")).Should().NotThrow();
        sut.Create(typeof(string), "").Should().NotBeNull();
    }

    [TestMethod]
    public void CreateTyped_ShouldNotThrowAndNotReturnNull()
    {
        IModelCacheFactory sut = new ModelCacheFactory();
        new Action(() => sut.Create("")).Should().NotThrow();
        sut.Create("").Should().NotBeNull();
    }
}
