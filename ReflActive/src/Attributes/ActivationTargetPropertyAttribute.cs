namespace ReflActive.Attributes;

/// <summary>
/// The <c>ActivationTargetPropertyAttribute</c> annotates <see cref="Type"/>s with additional data that is associated
/// with a unique  identifier.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ActivationTargetPropertyAttribute : Attribute
{
    /// <summary>
    /// The unique identifier for the data that this <c>ActivationTargetPropertyAttribute</c> annotates
    /// <see cref="Type"/>s with.
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// The data that this <c>ActivationTargetPropertyAttribute</c> annotates <see cref="Type"/>s with.
    /// </summary>
    public object? Value { get; init; } = null;
}
