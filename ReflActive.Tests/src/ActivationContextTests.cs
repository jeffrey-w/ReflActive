using ReflActive.Attributes;

namespace ReflActive.Tests;

[ActivationConfiguration]
public static class TestActivationConfiguration
{
    public static readonly bool Choice = true;
    [ActivationConfigurationAttribute.Constant]
    public static readonly int Count = 0;
}

public interface ITestDependency;

public class TestDependency : ITestDependency;

public class ActivationContextTests
{

    private IActivationContext _context;
    private ITestDependency _dependency;
    
    [SetUp]
    public void SetUp()
    {
        _context = ActivationContext.Init();
        _dependency = _context.AddDependency(new TestDependency());
    }

    [Test]
    public void AddDependencyThrowsWhenTypesOfSpecifiedValueIntersectWithExistingDependency()
    {
        Assert.Throws<ArgumentException>(() => _context.AddDependency(new TestDependency()));
    }

    [Test]
    public void GetDependencyReturnsExpectedValueWhenAnyValidTypeIsSpecified()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_context.GetDependency<ITestDependency>(), Is.SameAs(_dependency));
            Assert.That(_context.GetDependency<TestDependency>(), Is.SameAs(_dependency));
        });
    }

    [Test]
    public void GetDependencyThrowsWhenNoValueBelongsToSpecifiedType()
    {
        Assert.Throws<KeyNotFoundException>(() => _context.GetDependency<bool>());
    }

    [Test]
    public void DefineReturnsSpecifiedValueWhenNonExistentVariableIsSpecified()
    {
        Assert.That(_context.Define("Other", 0), Is.Zero);
    }

    [Test]
    public void DefineThrowsWhenExistingVariableIsSpecified()
    {
        Assert.Throws<ArgumentException>(() => _context.Define("Choice", false));
    }

    [Test]
    public void GetReturnsExpectedValueWhenCorrectTypeAndNameIsSpecified()
    {
        Assert.That(_context.Get<bool>("Choice"), Is.True);
    }

    [Test]
    public void GetThrowsWhenIncorrectTypeIsSpecified()
    {
        Assert.Throws<InvalidCastException>(() => _context.Get<int>("Choice"));
    }

    [Test]
    public void GetNonExistentVariableThrows()
    {
        Assert.Throws<KeyNotFoundException>(() => _context.Get<bool>("Other"));
    }

    [Test]
    public void SetReturnsSpecifiedValueWhenExistingNonConstantVariableIsSpecified()
    {
        Assert.That(_context.Set("Choice", false), Is.False);
    }

    [Test]
    public void SetThrowsWhenExistingConstantVariableIsSpecified()
    {
        Assert.Throws<InvalidOperationException>(() => _context.Set("Count", 1));
    }

    [Test]
    public void SetThrowsWhenNonExistentVariableIsSpecified()
    {
        Assert.Throws<KeyNotFoundException>(() => _context.Set("Other", 0));
    }
}
