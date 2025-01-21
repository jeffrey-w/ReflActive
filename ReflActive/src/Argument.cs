using System.Text.Json.Serialization;

namespace ReflActive;

/// <summary>
/// The <c>BaseArgument</c> class provides a minimal implementation for a binding to a constructor parameter.
/// </summary>
[JsonPolymorphic]
[JsonDerivedType(typeof(BooleanArgument), typeDiscriminator: "boolean")]
[JsonDerivedType(typeof(ContinuousNumberArgument), typeDiscriminator: "number")]
[JsonDerivedType(typeof(StringArgument), typeDiscriminator: "string")]
[JsonDerivedType(typeof(StringsArgument), typeDiscriminator: "strings")]
public abstract class BaseArgument
{
    /// <summary>
    /// The unique identifier for the constructor parameter targeted by this <c>BaseArgument</c>.
    /// </summary>
    public required string Name { get; init; }
    internal abstract object? Payload { get; }
}

/// <summary>
/// The <c>BaseTypedArgument</c> class provides a minimal implementation for a binding to a constructor parameter that
/// exhibits a definite <see cref="Type"/>.
/// </summary>
/// <typeparam name="TValue">The <see cref="Type"/> of <see cref="Value"/> specified by this <c>BaseTypedArgument</c>.</typeparam>
public abstract class BaseTypedArgument<TValue> : BaseArgument
{
    /// <summary>
    /// The data to bind to the constructor parameter targeted by this <c>BaseTypedArgument</c>.
    /// </summary>
    public TValue? Value { get; init; }
    
    internal override object? Payload => Value;
}

/// <summary>
/// The <c>BooleanArgument</c> class represents a binding to a Boolean-valued constructor parameter.
/// </summary>
public sealed class BooleanArgument : BaseTypedArgument<bool>;

/// <summary>
/// The <c>DiscreteNumberArgument</c> class represents a binding to a real-valued constructor parameter.
/// </summary>
public sealed class DiscreteNumberArgument : BaseTypedArgument<int>;

/// <summary>
/// The <c>ContinuousNumberArgument</c> class represents a binding to a real-valued constructor parameter.
/// </summary>
public sealed class ContinuousNumberArgument : BaseTypedArgument<double>;

/// <summary>
/// The <c>StringArgument</c> class represents a binding to a string-valued constructor parameter.
/// </summary>
public sealed class StringArgument : BaseTypedArgument<string>;

/// <summary>
/// The <c>StringsArgument</c> class represents a binding to a composite string-valued constructor parameter.
/// </summary>
public sealed class StringsArgument : BaseTypedArgument<List<string>>;
