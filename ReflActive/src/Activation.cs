namespace ReflActive;

/// <summary>
/// The <c>Activation</c> class represents a constructor invocation.
/// </summary>
public sealed class Activation
{
    /// <summary>
    /// The prefix of the unique identifier for a class, the constructor to which this <c>Activation</c> targets.
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// The suffix of the unique identifier for a class, the constructor to which this <c>Activation</c> targets.
    /// </summary>
    public string Discriminator { get; init; } = string.Empty;
    /// <summary>
    /// <see cref="BaseArgument">Bindings</see> to the constructor parameters targeted by this <c>Activation</c>.
    /// </summary>
    public required List<BaseArgument> Arguments { get; init; }

    internal Dictionary<string, object?> ToBindings()
    {
        return Arguments.ToDictionary(argument => argument.Name, argument => argument.Payload);
    }
}