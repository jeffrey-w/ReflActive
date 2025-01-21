namespace ReflActive.Attributes;

/// <summary>
/// The <c>ParameterAttribute</c> annotates parameters to a constructor exhibiting the <see cref="ActivationTargetConstructorAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ParameterAttribute : Attribute
{
    /// <summary>
    /// The unique identifier for the parameter annotated by this <c>ParameterAttribute</c>.
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// The natural language characterization for the parameter annotated by this <c>ParameterAttribute</c>.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
