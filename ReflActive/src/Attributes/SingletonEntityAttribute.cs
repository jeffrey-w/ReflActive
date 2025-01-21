namespace ReflActive.Attributes;

/// <summary>
/// The <c>CompositeEntityAttribute</c> annotates a parameter to a constructor exhibiting the <see cref="ActivationTargetConstructorAttribute"/>
/// that ranges over a subset of a <see cref="Type"/> for which each member is assigned a unique identifier.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class SingletonEntityAttribute : Attribute
{
    /// <summary>
    /// The <see cref="Type"/> over which the parameter annotated by this <c>SingletonEntityAttribute</c> ranges.
    /// </summary>
    public required Type Type
    {
        get => _type;
        init => _type = Guard.Against.InvalidType<ISingletonEntityConverter>(value);
    }
    
    private readonly Type _type = null!;

    internal string GetDefault(IActivationContext context)
    {
        return EntityConverter.Singleton(_type, context)
                              .Default;
    }

    internal IEnumerable<string> GetIds(IActivationContext context)
    {
        return EntityConverter.Singleton(_type, context)
                              .Ids;
    }

    internal object GetEntity(string id, IActivationContext context)
    {
        return EntityConverter.Singleton(_type, context)
                              .Entity(id);
    }
}
