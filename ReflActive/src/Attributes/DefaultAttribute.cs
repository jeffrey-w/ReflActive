namespace ReflActive.Attributes;

/// <summary>
/// The <c>DefaultAttribute</c> annotates parameters to a constructor exhibiting the <see cref="ActivationTargetConstructorAttribute"/>
/// with a value to which it is typically bound.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public abstract class DefaultAttribute : Attribute
{
    internal abstract object? GetValue(IActivationContext context);
}

/// <summary>
/// The <c>StaticDefaultAttribute</c> annotates parameters to a constructor exhibiting the <see cref="ActivationTargetConstructorAttribute"/>
/// with a value to which it is typically bound, and is resolved at compile time.
/// </summary>
public sealed class StaticDefaultAttribute : DefaultAttribute
{
    /// <summary>
    /// The data typically bound to the parameter annotated by this <c>StaticDefaultAttribute</c>.
    /// </summary>
    public required object? Value { get; init; }
    
    internal override object? GetValue(IActivationContext context)
    {
        return Value;
    }
}

/// <summary>
/// The <c>DynamicDefaultAttribute</c> annotates parameters to a constructor exhibiting the <see cref="ActivationTargetConstructorAttribute"/>
/// with a value to which it is typically bound, and is resolved at run time.
/// </summary>
public sealed class DynamicDefaultAttribute : DefaultAttribute
{
    /// <summary>
    /// The unique identifier for the data typically bound to the parameter annotated by this <c>DynamicDefaultAttribute</c>
    /// in the current <see cref="IActivationContext"/>.
    /// </summary>
    /// <exception cref="ArgumentException">If the value to which this property is initialized is <c>null</c> or consists
    /// solely of whitespace.</exception>
    /// <exception cref="KeyNotFoundException">If there is no value so identified in the current <see cref="IActivationContext"/>.</exception>
    public required string Name
    {
        get => _name;
        init => _name = Guard.Against.NullOrWhitespace(value);
    }

    private readonly string _name = null!;

    internal override object? GetValue(IActivationContext context)
    {
        return context.Get<object>(_name);
    }
}
