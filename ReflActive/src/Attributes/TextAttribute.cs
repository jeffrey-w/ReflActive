namespace ReflActive.Attributes;

/// <summary>
/// The <c>TextAttribute</c> annotates parameters to a constructor exhibiting the <see cref="ActivationTargetConstructorAttribute"/>
/// that may be bound to a string.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class TextAttribute : Attribute
{
    /// <summary>
    /// The least number of characters the string bound to the parameter annotated by this <c>TextAttribute</c> may have.
    /// </summary>
    public int Min { get; init; } = 0;

    /// <summary>
    /// The greatest number of characters the string bound to the parameter annotated by this <c>TextAttribute</c> may
    /// have.
    /// </summary>
    public int Max { get; init; } = int.MaxValue;
    /// <summary>
    /// The regular expression that defines the strings that may be bound to the parameter annotated by this <c>TextAttribute</c>.
    /// </summary>
    public string? Pattern { get; init; }
}