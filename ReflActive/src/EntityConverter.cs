namespace ReflActive;

/// <summary>
/// The <c>IEntityConverter</c> interface provides properties and operations on a facility for projecting members of a
/// <see cref="Type"/> to and from their unique identifiers.
/// </summary>
public interface IEntityConverter
{
    /// <summary>
    /// The unique identifiers for the members of the <see cref="Type"/> over which this <c>IEntityConverter</c>
    /// operates.
    /// </summary>
    public IEnumerable<string> Ids { get; }
}

/// <summary>
/// The <c>ISingletonEntityConverter</c> interface provides properties and operations on an <see cref="IEntityConverter"/>
/// that facilitates selection of a single member of the <see cref="Type"/> over which it operates.
/// </summary>
public interface ISingletonEntityConverter : IEntityConverter
{
    /// <summary>
    /// The unique identifier for the member of the <see cref="Type"/> over which this <c>ISingletonEntityConverter</c>
    /// operates that is typically selected.
    /// </summary>
    public string Default { get; }
    
    /// <summary>
    /// Provides the member of the <see cref="Type"/> over which this <c>ISingletonEntityConverter</c> operates that is
    /// identified by the specified <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The unique identifiers for a member of the <see cref="Type"/> over which this <c>ISingletonEntityConverter</c>
    /// operates.</param>
    /// <returns>The entity identified by the specified <paramref name="id"/>.</returns>
    public object Entity(string id);
}

/// <summary>
/// The <c>ISingletonEntityConverter</c> interface provides properties and operations on an <see cref="IEntityConverter"/>
/// that facilitates selection of a subset of the <see cref="Type"/> over which it operates.
/// </summary>
public interface ICompositeEntityConverter : IEntityConverter
{
    /// <summary>
    /// The unique identifiers for the members of the <see cref="Type"/> over which this <c>ICompositeEntityConverter</c>
    /// operates that are typically selected.
    /// </summary>
    IEnumerable<string> Default { get; }
    
    /// <summary>
    /// Provides the members of the <see cref="Type"/> over which this <c>ICompositeEntityConverter</c> operates that are
    /// identified by the specified <paramref name="ids"/>.
    /// </summary>
    /// <param name="ids">The unique identifiers for a subset of the members of the <see cref="Type"/> over which
    /// this <c>ICompositeEntityConverter</c> operates.</param>
    /// <returns>The entities identified by the specified <paramref name="ids"/>.</returns>
    object Entities(IEnumerable<string> ids);
}

internal static class EntityConverter
{
    public static ISingletonEntityConverter Singleton(Type type, IActivationContext context)
    {
        return MakeConverter<ISingletonEntityConverter>(type, context);
    }
    
    public static ICompositeEntityConverter Composite(Type type, IActivationContext context)
    {
        return MakeConverter<ICompositeEntityConverter>(type, context);
    }

    private static T MakeConverter<T>(Type type, IActivationContext context) where T : IEntityConverter
    {
        return (T?)Guard.Against
                   .InvalidType<T>(type)
                   .GetConstructor([typeof(IActivationContext)])?
                   .Invoke([context]) ??
               throw new InvalidOperationException();
    }
}
