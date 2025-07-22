using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json.Serialization;
using Extensions;
using ReflActive.Attributes;

namespace ReflActive;

/// <summary>
/// The <c>IActivationTargetMetadata</c> interface provides properties and operations on descriptive attributes for a
/// <see cref="Type"/> that may be <see cref="Activator.Activate{TResult}">activated</see>.
/// </summary>
[JsonDerivedType(typeof(ActivationTargetMetadata.SingletonActivationTargetMetadata))]
[JsonDerivedType(typeof(ActivationTargetMetadata.CompositeActivationTargetMetadata))]
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
    /// Indicates whether the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c> is ready to be tested
    /// in production environments.
    /// </summary>
    public bool IsExperimental { get; }
    /// <summary>
    /// Indicates whether the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c> is modulated by
    /// varying input values to its constructor.
    /// </summary>
    public bool IsParameterized { get; }
    /// <summary>
    /// Indicates whether this <c>IActivationTargetMetadata</c> is logically composed by one or more other instances.
    /// </summary>
    public bool IsComposite { get; }
    /// <summary>
    /// Additional attributes associated with the <see cref="Type"/> described by this <c>IActivationTargetMetadata</c>. 
    /// </summary>
    public IDictionary<string, string> Properties { get; }
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
/// The <c>ICompositeActivationTargetMetadata</c> interface provides properties and operations on <see cref="IActivationTargetMetadata"/>
/// that is logically composed by one or more other instances.
/// </summary>
public interface ICompositeActivationTargetMetadata : IActivationTargetMetadata
{
    /// <summary>
    /// Descriptions of the <see cref="Types"/> that logically compose the one described by this <c>ICompositeActivationTargetMetadata</c>.
    /// </summary>
    public IEnumerable<IActivationTargetMetadata> Children { get; }
}

/// <summary>
/// The <c>ActivationTargetMetadata</c> class provides a concrete implementation of the <see cref="IActivationTargetMetadata"/>
/// interface.
/// </summary>
public static class ActivationTargetMetadata
{
    internal sealed class SingletonActivationTargetMetadata : IActivationTargetMetadata
    {
        public string Name => _type.GetCustomAttribute<ActivationTargetAttribute>()?.Name ?? string.Empty;
        public string Discriminator => _type.GetCustomAttribute<ActivationTargetAttribute>()?.Discriminator ?? string.Empty;
        public string Description => _type.GetCustomAttribute<ActivationTargetAttribute>()?.Description ?? string.Empty;
        public bool IsDevelopment => _type.GetCustomAttribute<ActivationTargetAttribute>()?.IsDevelopment ?? false;
        public bool IsExperimental => _type.GetCustomAttribute<ActivationTargetAttribute>()?.IsExperimental ?? false;
        public bool IsComposite => false;
        public bool IsParameterized => 
            Toggles.Any() || Counts.Any() || Quantities.Any() || Labels.Any() || SingleSelections.Any() || CompositeSelections.Any();

        public IDictionary<string, string> Properties => _properties;
        public IEnumerable<IBooleanParameter> Toggles => GetParameters(IsAssignableTo<bool>).Select(_factory.MakeBoolean);
        public IEnumerable<IDiscreteNumberParameter> Counts => 
            GetParameters(IsAssignableTo<int>).Select(_factory.MakeDiscreteNumber);
        public IEnumerable<IContinuousNumberParameter> Quantities => 
            GetParameters(IsAssignableTo<double>).Select(_factory.MakeContinuousNumber);
        public IEnumerable<ITextParameter> Labels => GetParameters(IsAssignableTo<string>).Select(_factory.MakeText);
        public IEnumerable<ISingletonEntityParameter> SingleSelections =>
            GetParameters(IsAnnotatedBy<SingletonEntityAttribute>).Select(_factory.MakeSingletonEntity);
        public IEnumerable<ICompositeEntityParameter> CompositeSelections =>
            GetParameters(IsAnnotatedBy<CompositeEntityAttribute>).Select(_factory.MakeCompositeEntity);

        private readonly Parameter.Factory _factory;

        private readonly ImmutableDictionary<string, string> _properties;

        private readonly Type _type;

        public SingletonActivationTargetMetadata(Type type, IActivationContext context, params string[] exclude)
        {
            _type = Guard.Against.Violation(type, t => IsValidType(t, context));
            _factory = new Parameter.Factory(context);
            _properties = CompileProperties(type, exclude);
        }
        
        private static bool IsValidType(Type type, IActivationContext context)
        {
            return type
                   .GetCustomAttribute<ActivationTargetAttribute>()
                   ?.CanBeActivatedIn(context) ??
                   false;
        }

        private static ImmutableDictionary<string, string> CompileProperties(Type type, IEnumerable<string> exclude)
        {
            return type
                   .GetCustomAttribute<PropertiesAttribute>()
                   ?.Compile(exclude.ToHashSet()) ??
                   ImmutableDictionary.Create<string, string>();
        }

        private IEnumerable<ParameterInfo> GetParameters(Func<ParameterInfo, bool> predicate)
        {
            return _type
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

        internal IActivationTargetMetadata WithoutProperties(IEnumerable<string> names)
        {
            return new SingletonActivationTargetMetadata(_type, _factory.Context, names.ToArray());
        }
    }

    internal sealed class CompositeActivationTargetMetadata : ICompositeActivationTargetMetadata
    {
        public string Name { get; }
        public string Discriminator => string.Empty;
        public string Description => string.Empty;
        public bool IsDevelopment => Children.All(child => child.IsDevelopment);
        public bool IsExperimental => Children.All(child => child.IsExperimental);
        public bool IsParameterized => false;
        public bool IsComposite => true;
        public IDictionary<string, string> Properties { get; }
        public IEnumerable<IBooleanParameter> Toggles => [];
        public IEnumerable<IDiscreteNumberParameter> Counts => [];
        public IEnumerable<IContinuousNumberParameter> Quantities => [];
        public IEnumerable<ITextParameter> Labels => [];
        public IEnumerable<ISingletonEntityParameter> SingleSelections => [];
        public IEnumerable<ICompositeEntityParameter> CompositeSelections => [];

        public IEnumerable<IActivationTargetMetadata> Children =>
            _children
                .Cast<SingletonActivationTargetMetadata>()
                .Select(child => child.WithoutProperties(Properties.Keys))
                .ToList();
        
        private readonly List<IActivationTargetMetadata> _children;

        public CompositeActivationTargetMetadata(string name, IEnumerable<IActivationTargetMetadata> children)
        {
            _children = children.ToList();
            Name = name;
            Properties = CompileProperties();
        }

        private ImmutableDictionary<string, string> CompileProperties()
        {
            return _children
                   .Skip(1)
                   .Select(child => child.Properties)
                   .Aggregate(
                       _children
                           .First()
                           .Properties
                           .AsEnumerable(),
                       (intersection, properties) => intersection.Where(pair => Contains(properties, pair)),
                       intersection => intersection.ToImmutableDictionary());
        }

        private static bool Contains(IDictionary<string, string> properties, KeyValuePair<string, string> pair)
        {
            return properties.TryGetValue(pair.Key, out var value) && value == pair.Value;
        }
    }

    /// <summary>
    /// Provides a new <see cref="IActivationTargetMetadata"/> instance for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> described by the new <see cref="IActivationTargetMetadata"/> instance.</param>
    /// <param name="context">The current <see cref="IActivationContext"/>.</param>
    /// <returns>A new <see cref="IActivationTargetMetadata"/> instance.</returns>
    /// <exception cref="ArgumentException">If the specified <paramref name="type"/> does not exhibit the
    /// <see cref="ActivationTargetAttribute"/>, or if it is unsupported by the specified <paramref name="context"/>.</exception>
    public static IActivationTargetMetadata Singleton(Type type, IActivationContext context)
    {
        return new SingletonActivationTargetMetadata(type, context);
    }

    /// <summary>
    /// Provides a new <see cref="ICompositeActivationTargetMetadata"/> instance for the specified <paramref name="children"/>.
    /// </summary>
    /// <param name="name">The identifier common to each of the specified <paramref name="children"/>.</param>
    /// <param name="children">The <see cref="IActivationTargetMetadata"/> instances related by the new
    /// <see cref="ICompositeActivationTargetMetadata"/>.</param>
    /// <returns>A new <see cref="ICompositeActivationTargetMetadata"/> instance.</returns>
    /// <exception cref="ArgumentException">If any of the specified <paramref name="children"/> are <c>null</c>, or if any
    /// of them do not exhibit the specified <paramref name="name"/>.</exception>
    public static ICompositeActivationTargetMetadata Composite(string name, IEnumerable<IActivationTargetMetadata> children)
    {
        return new CompositeActivationTargetMetadata(
            name,
            Guard
                .Against
                .InvalidEnumerable(children)
                .NullElements()
                .AnyViolation(child => child.Name == name)
                .Validated());
    }

    /// <summary>
    /// Provides <see cref="IActivationTargetMetadata"/> instanes for every member of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type for which <see cref="IActivationTargetMetadata"/> is being queried.</typeparam>
    /// <param name="context">The current <see cref="IActivationContext"/>.</param>
    /// <returns>A collection of <see cref="IActivationTargetMetadata"/> instances describing the members of
    /// <typeparamref name="T"/>.</returns>
    public static IEnumerable<IActivationTargetMetadata> For<T>(IActivationContext context)
    {
        return EveryType
               .That
               .IsAssignableTo<T>()
               .Where(type => CanBeActivatedIn(type, context))
               .GroupBy(type => type.GetCustomAttribute<ActivationTargetAttribute>()!)
               .Select(grouping => From(grouping, context));
    }

    private static bool CanBeActivatedIn(Type type, IActivationContext context)
    {
        // TODO figure out a way to remove this
        return type
               .GetCustomAttribute<ActivationTargetAttribute>()
               ?.CanBeActivatedIn(context) ??
               false;
    }

    private static IActivationTargetMetadata From(
        IGrouping<ActivationTargetAttribute, Type> grouping, IActivationContext context)
    {
        return grouping.IsSingleton()
            ? new SingletonActivationTargetMetadata(grouping.Single(), context)
            : new CompositeActivationTargetMetadata(
                grouping.Key.Name,
                grouping.Select(type => new SingletonActivationTargetMetadata(type, context)));
    }
}
