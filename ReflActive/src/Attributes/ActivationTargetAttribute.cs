namespace ReflActive.Attributes;

/// <summary>
/// The <c>ActivationTargetAttribute</c> annotates <see cref="Type"/>s that may be <see
/// cref="Activator.Activate{TResult,TAttribute}">activated</see> with descriptive information.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public abstract class ActivationTargetAttribute : Attribute, IEquatable<ActivationTargetAttribute>
{
    /// <summary>
    /// The prefix of the unique identifier for the <see cref="Type"/> annotated by this <c>ActivationTargetAttribute</c>.
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// The suffix of the unique identifier for the <see cref="Type"/> annotated by this <c>ActivationTargetAttribute</c>.
    /// </summary>
    public string Discriminator { get; init; } = string.Empty;
    /// <summary>
    /// The natural language characterization for the <see cref="Type"/> annotated by this <c>ActivationTargetAttribute</c>.
    /// </summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>
    /// Indicates whether the <see cref="Type"/> annotated by this <c>ActivationTargetAttribute</c> is not intended for
    /// production environments.
    /// </summary>
    public bool IsDevelopment { get; init; }
    /// <summary>
    /// Indicates whether the <see cref="Type"/> annotated by this <c>ActivationTargetAttribute</c> is ready to be tested
    /// in production environments.
    /// </summary>
    public bool IsExperimental { get; init; }
    /// <summary>
    /// Indicates that the <see cref="Type"/> annotated by this <c>ActivationTargetAttribute</c> ought to be preferred
    /// when selecting those to <see cref="Activator.Activate{TResult,TAttribute}">activate</see>.
    /// </summary>
    public bool IsPermanent { get; init; }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ActivationTargetAttribute);
    }

    /// <inheritdoc/>
    public bool Equals(ActivationTargetAttribute? other)
    {
        return Name.Equals(other?.Name);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name);
    }

    internal bool IsTargetedBy(Activation activation)
    {
        return Name.Equals(activation.Name) && Discriminator.Equals(activation.Discriminator);
    }
}
