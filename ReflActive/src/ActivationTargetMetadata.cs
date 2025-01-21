using System.Reflection;
using Extensions;
using ReflActive.Attributes;

namespace ReflActive;

/// <summary>
/// The <c>IActivationTargetMetadata</c> interface provides properties and operations on descriptive attributes for a
/// <see cref="Type"/> that may be <see cref="Activator.Activate{TResult,TAttribute}">activated</see>.
/// </summary>
public interface IActivationTargetMetadata
{
    /// <summary>
    /// The prefix of the unique identifier for the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c>.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The suffix of the unique identifier for the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c>.
    /// </summary>
    public string Discriminator { get; }
    /// <summary>
    /// The natural language characterization for the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c>.
    /// </summary>
    public string Description { get; }
    /// <summary>
    /// Indicates whether the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c> is not intended for
    /// production environments.
    /// </summary>
    public bool IsDevelopment { get; }
    /// <summary>
    /// Indicates whether the <see cref="Type"/> annotated by this <c>IActivationTargetMetadata</c> is ready to be tested
    /// in production environments.
    /// </summary>
    public bool IsExperimental { get; }
    /// <summary>
    /// Indicates whether the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c> is modulated by
    /// varying input values to its constructor.
    /// </summary>
    public bool IsParameterized { get; }
    /// <summary>
    /// The Boolean-valued <see cref="IParameter{TDomain}">parameters</see> to the <see cref="ActivationTargetConstructorAttribute">
    /// constructor</see> for the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c>.
    /// </summary>
    public IEnumerable<IBooleanParameter> Toggles { get; }
    /// <summary>
    /// The integer-valued <see cref="IParameter{TDomain}">parameters</see> to the <see cref="ActivationTargetConstructorAttribute">
    /// constructor</see> for the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c>.
    /// </summary>
    public IEnumerable<IDiscreteNumberParameter> Counts { get; }
    /// <summary>
    /// The real-valued <see cref="IParameter{TDomain}">parameters</see> to the <see cref="ActivationTargetConstructorAttribute">
    /// constructor</see> for the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c>.
    /// </summary>
    public IEnumerable<IContinuousNumberParameter> Quantities { get; }
    /// <summary>
    /// The string-valued <see cref="IParameter{TDomain}">parameters</see> to the <see cref="ActivationTargetConstructorAttribute">
    /// constructor</see> for the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c>.
    /// </summary>
    public IEnumerable<ITextParameter> Labels { get; }
    /// <summary>
    /// The <see cref="IParameter{TDomain}">parameters</see> to the <see cref="ActivationTargetConstructorAttribute">constructor</see>
    /// for the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c> that may be bound to a value from a
    /// finite subset of strings that identify the members of another <see cref="Type"/>.
    /// </summary>
    public IEnumerable<ISingletonEntityParameter> SingleSelections { get; }
    /// <summary>
    /// The <see cref="IParameter{TDomain}">parameters</see> to the <see cref="ActivationTargetConstructorAttribute">constructor</see>
    /// for the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c> that may be bound to multiple
    /// values from a finite subset of strings that identify the members of another <see cref="Type"/>.
    /// </summary>
    public IEnumerable<ICompositeEntityParameter> CompositeSelections { get; }
}

/// <summary>
/// The <c>BaseActivationTargetMetadata</c> class provides a minimal implementation of the <see cref="IActivationTargetMetadata"/>
/// interface.
/// </summary>
public abstract class BaseActivationTargetMetadata : IActivationTargetMetadata
{
    /// <inheritdoc/>
    public string Name => Type.GetCustomAttribute<ActivationTargetAttribute>()?.Name ?? string.Empty;
    /// <inheritdoc/>
    public string Discriminator => Type.GetCustomAttribute<ActivationTargetAttribute>()?.Discriminator ?? string.Empty;
    /// <inheritdoc/>
    public string Description => Type.GetCustomAttribute<ActivationTargetAttribute>()?.Description ?? string.Empty;
    /// <inheritdoc/>
    public bool IsDevelopment => Type.GetCustomAttribute<ActivationTargetAttribute>()?.IsDevelopment ?? false;
    /// <inheritdoc/>
    public bool IsExperimental => Type.GetCustomAttribute<ActivationTargetAttribute>()?.IsExperimental ?? false;
    /// <inheritdoc/>
    public bool IsParameterized => 
        Toggles.Any() || Counts.Any() || Quantities.Any() || Labels.Any() || SingleSelections.Any() || CompositeSelections.Any();
    /// <inheritdoc/>
    public IEnumerable<IBooleanParameter> Toggles => GetParameters(IsAssignableTo<bool>).Select(_factory.MakeBoolean);
    /// <inheritdoc/>
    public IEnumerable<IDiscreteNumberParameter> Counts => 
        GetParameters(IsAssignableTo<int>).Select(_factory.MakeDiscreteNumber);
    /// <inheritdoc/>
    public IEnumerable<IContinuousNumberParameter> Quantities => 
        GetParameters(IsAssignableTo<double>).Select(_factory.MakeContinuousNumber);
    /// <inheritdoc/>
    public IEnumerable<ITextParameter> Labels => GetParameters(IsAssignableTo<string>).Select(_factory.MakeText);
    /// <inheritdoc/>
    public IEnumerable<ISingletonEntityParameter> SingleSelections =>
        GetParameters(IsAnnotatedBy<SingletonEntityAttribute>).Select(_factory.MakeSingletonEntity);
    /// <inheritdoc/>
    public IEnumerable<ICompositeEntityParameter> CompositeSelections =>
        GetParameters(IsAnnotatedBy<CompositeEntityAttribute>).Select(_factory.MakeCompositeEntity);
    
    /// <summary>
    /// The <see cref="System.Type"/> described by this <c>BaseActivationTargetMetadata</c>.
    /// </summary>
    protected Type Type { get; }

    private readonly Parameter.Factory _factory;

    /// <summary>
    /// Creates a new <c>BaseActivationTargetMetadata</c>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> described by this <c>BaseActivationTargetMetadata</c>.</param>
    /// <param name="context">The current <see cref="IActivationContext"/>.</param>
    protected BaseActivationTargetMetadata(Type type, IActivationContext context)
    {
        Type = type;
        _factory = new Parameter.Factory(context);
    }
    
    private IEnumerable<ParameterInfo> GetParameters(Func<ParameterInfo, bool> predicate)
    {
        return Type
            .GetConstructors()
            .Single(constructor => constructor.HasCustomAttribute<ActivationTargetConstructorAttribute>())
            .GetParameters()
            .Where(predicate);
    }
    
    private static bool IsAssignableTo<T>(ParameterInfo parameter)
    {
        return (Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType) == typeof(T);
    }

    private static bool IsAnnotatedBy<T>(ParameterInfo info) where T : Attribute
    {
        return info.HasCustomAttribute<T>();
    }
}
