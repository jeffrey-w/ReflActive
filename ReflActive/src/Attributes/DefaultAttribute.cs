using Extra.Guard;

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
    private static readonly object Sentinel = new();
    
    /// <summary>
    /// The unique identifier for the data typically bound to the parameter annotated by this <c>DynamicDefaultAttribute</c>
    /// in the current <see cref="IActivationContext"/>.
    /// </summary>
    /// <exception cref="ArgumentException">If the value to which this property is initialized is <c>null</c> or consists
    /// solely of whitespace.</exception>
    /// <exception cref="KeyNotFoundException">If there is no value so identified in the current <see cref="IActivationContext"/>,
    /// nor a <see cref="FallbackValue"/> associated with this <c>DynamicDefaultAttribute</c>.</exception>
    public required string Name
    {
        get => _name;
        init => _name = Against.NullOrWhitespace(value);
    }

    /// <summary>
    /// The data bound to the parameter annotated by this <c>DynamicDefaultAttribute</c>
    /// when the <see cref="Name"/> associated with it is not declared in the
    /// current <see cref="IActivationContext"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">If this value is queried without
    /// having been explicitly defined.</exception>
    public object? FallbackValue
    {
        get => IsFallbackValueSet ? _fallbackValue : throw new InvalidOperationException(); // TODO
        init => _fallbackValue = value;
    }

    private bool IsFallbackValueSet => _fallbackValue != Sentinel;

    private readonly string _name = null!;
    private readonly object? _fallbackValue = Sentinel;

    internal override object? GetValue(IActivationContext context)
    {
        try
        {
            return context.Get<object>(_name);
        }
        catch (KeyNotFoundException)
        {
            if (IsFallbackValueSet)
            {
                return FallbackValue;
            }
            throw;
        }
    }
}
