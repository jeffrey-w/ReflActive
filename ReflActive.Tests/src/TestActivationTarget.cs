using ReflActive.Attributes;

namespace ReflActive.Tests;

// ReSharper disable UnusedParameter.Local

public class Entity
{
    public required string Id { get; init; }
}

#pragma warning disable CS9113 // Parameter is unread.
public class SingletonEntityConverter(IActivationContext _) : ISingletonEntityConverter
#pragma warning restore CS9113 // Parameter is unread.
{
    public IEnumerable<string> Ids => _entities.Keys;
    public string Default => "X";

    private readonly Dictionary<string, Entity> _entities = new()
    {
        { "X", new Entity { Id = "X" } }, { "Y", new Entity { Id = "Y" } }, { "Z", new Entity { Id = "Z" } },
    };
    
    public object Entity(string id)
    {
        return _entities[id];
    }
}

public class InvalidSingletonEntityConverter : ISingletonEntityConverter
{
    public IEnumerable<string> Ids => _entities.Keys;
    public string Default => "X";
    
    private readonly Dictionary<string, Entity> _entities = new()
    {
        { "X", new Entity { Id = "X" } }, { "Y", new Entity { Id = "Y" } }, { "Z", new Entity { Id = "Z" } },
    };
    
    public object Entity(string id)
    {
        return _entities[id];
    }
}

#pragma warning disable CS9113 // Parameter is unread.
public class CompositeEntityConverter(IActivationContext _) : ICompositeEntityConverter
#pragma warning restore CS9113 // Parameter is unread.
{
    public IEnumerable<string> Ids => _entities.Keys;
    public IEnumerable<string> Default => ["Y", "Z"];
    
    private readonly Dictionary<string, Entity> _entities = new()
    {
        { "X", new Entity { Id = "X" } }, { "Y", new Entity { Id = "Y" } }, { "Z", new Entity { Id = "Z" } },
    }; 
    
    public object Entities(IEnumerable<string> ids)
    {
        return ids
               .Select(id => _entities[id])
               .ToHashSet();
    }
}


[ActivationTarget(Name = "Test", Description = "Test", IsDevelopment = true)]
[ActivationTargetProperty(Name = "One", Value = 1)]
[ActivationTargetProperty(Name = "Two", Value = 2)]
public class TestActivationTarget
{
    [ActivationVariable(Name = "Default Count")]
    public static readonly int DefaultCount = 10;

    [ActivationVariable(Name = "Quantity Max")]
    public static readonly double QuantityMax = 10f;
    
    public bool? B { get; }
    public int I { get; }
    public double D { get; }
    public string S { get; }
    public Entity E { get; }
    public ISet<Entity> Es { get; }

    [ActivationTargetConstructor]
    public TestActivationTarget(
        [Parameter(Name = "B", Description = "Toggle")]
        [StaticDefault(Value = true)]
        bool? b,
        [Parameter(Name = "I", Description = "Count")]
        [DynamicDefault(Name = "Default Count")]
        [DiscreteNumber(0, 10, Step = 2)]
        int i,
        [Parameter(Name = "D", Description = "Quantity")]
        [ContinuousNumber(double.MinValue, "Quantity Max", Precision = 3)]
        double d,
        [Parameter(Name = "S", Description = "Label")]
        [Text(Min = 1, Max = 10, Pattern = @"[A-Za-z_]\w")]
        string s,
        [Parameter(Name = "E", Description = "Single Selection")]
        [SingletonEntity(Type = typeof(SingletonEntityConverter))]
        Entity e,
        [Parameter(Name = "Es", Description = "Composite Selection")]
        [CompositeEntity(Type = typeof(CompositeEntityConverter))]
        ISet<Entity> es)
    {
        B = b;
        I = i;
        D = d;
        S = s;
        E = e;
        Es = es;
    }
}

[ActivationTarget(Name = "Undefined Dynamic Default With Fallback Value")]
public class UndefinedDynamicDefaultWithFallbackValueActivationTarget
{
    [ActivationTargetConstructor]
    public UndefinedDynamicDefaultWithFallbackValueActivationTarget(
        [Parameter(Name = "I")] [DynamicDefault(Name = "Undefined", FallbackValue = int.MaxValue)] int i)
    {
    }
}

[ActivationTarget(Name = "Undefined Dynamic Default Without Fallback Value")]
public class UndefinedDynamicDefaultWithoutFallbackValueActivationTarget
{
    [ActivationTargetConstructor]
    public UndefinedDynamicDefaultWithoutFallbackValueActivationTarget(
        [Parameter(Name = "I")] [DynamicDefault(Name = "Undefined")] int i)
    {
    }
}

[ActivationTarget(Name = "Other")]
public class OtherTestActivationTarget
{
    [ActivationTargetConstructor]
    public OtherTestActivationTarget()
    {
    }
}

[ActivationTarget(Name = "Duplicate Parameters")]
public class DuplicateParametersActivationTarget
{
    [ActivationTargetConstructor]
    public DuplicateParametersActivationTarget([Parameter(Name = "B")] bool b, [Parameter(Name = "B")] bool c)
    {
    }
}

public interface IDuplicateNameActivationTarget;

[ActivationTarget(Name = "Duplicate Name")]
public class DuplicateNameActivationTarget1 : IDuplicateNameActivationTarget
{
    [ActivationTargetConstructor]
    public DuplicateNameActivationTarget1()
    {
    }
}

[ActivationTarget(Name = "Duplicate Name")]
public class DuplicateNameActivationTarget2 : IDuplicateNameActivationTarget
{
    [ActivationTargetConstructor]
    public DuplicateNameActivationTarget2()
    {
    }
}

[ActivationTarget(Name = "No Constructor")]
public class NoConstructorActivationTarget;

[ActivationTarget(Name = "Multiple Constructors")]
public class MultipleConstructors
{
    [ActivationTargetConstructor]
    public MultipleConstructors()
    {
    }

    [ActivationTargetConstructor]
    public MultipleConstructors(bool b)
    {
    }
}

[ActivationTarget(Name = "Missing Attribute")]
public class MissingAttributeActivationTarget
{
    [ActivationTargetConstructor]
    public MissingAttributeActivationTarget(bool b)
    {
    }
}

[ActivationTarget(Name = "Invalid Converter")]
public class InvalidConverterActivationTarget
{
    [ActivationTargetConstructor]
    public InvalidConverterActivationTarget(
        [Parameter(Name = "E")] [SingletonEntity(Type = typeof(InvalidSingletonEntityConverter))] Entity e)
    {
    }
}
