using Extra.Guard;

namespace ReflActive.Attributes;

/// <summary>
/// The <c>CompositeEntityAttribute</c> annotates a parameter to a constructor exhibiting the <see cref="ActivationTargetConstructorAttribute"/>
/// that ranges over a subset of a <see cref="Type"/> for which each member is assigned a unique identifier, and may be
/// bound to more than one simultaneously.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class CompositeEntityAttribute : Attribute
{
    /// <summary>
    /// The <see cref="Type"/> over which the parameter annotated by this <c>CompositeEntityAttribute</c> ranges.
    /// </summary>
    /// <exception cref="ArgumentException">If the <see cref="Type"/> used to initialize this property does not
    /// implement the <see cref="ICompositeEntityConverter"/> interface.</exception>
    public required Type Type
    {
        get => _type;
        init => _type = Against.InvalidType<ICompositeEntityConverter>(value);
    }
    
    private readonly Type _type = null!;

    internal IEnumerable<string> GetDefaults(IActivationContext context)
    {
        return EntityConverter.Composite(_type, context)
                              .Default;
    }

    internal IEnumerable<string> GetIds(IActivationContext context)
    {
        return EntityConverter.Composite(_type, context)
                              .Ids;
    }

    internal object GetEntities(IEnumerable<string> value, IActivationContext context)
    {
        return EntityConverter.Composite(_type, context)
                              .Entities(value);
    }
}