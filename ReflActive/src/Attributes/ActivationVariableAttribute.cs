namespace ReflActive.Attributes;

/// <summary>
/// The <c>ActivationVariableAttribute</c> class identifies a value to include in the current <see cref="IActivationContext"/>.
/// </summary>
/// <remarks>Only public, static values annotated by this <see cref="Attribute"/> are included in the current <see
/// cref="IActivationContext"/>. Values are associated with a <see cref="Name"/>, and it is imperative that identifiers be
/// unique. If two values are associated with the same name, an <see cref="InvalidOperationException"/> is thrown.</remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class ActivationVariableAttribute : Attribute
{
    /// <summary>
    /// The unique identifier for the value annotated by this <c>ActivationVariableAttribute</c>.
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// Indicates whether this <c>ActivationVariableAttribute</c> annotates a value that may not be changed once it is
    /// defined in this current <see cref="IActivationContext"/>.
    /// </summary>
    public bool IsConstant { get; init; } = true;
    /// <summary>
    /// Indicates whether this <c>ActivationVariableAttribute</c> annotates a value that is not intended for production.
    /// </summary>
    public bool IsDevelopment { get; init; } = false;
}
