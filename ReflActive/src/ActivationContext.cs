using System.Reflection;
using Extensions;
using ReflActive.Attributes;

namespace ReflActive;

/// <summary>
/// The <c>IActivationContext</c> interface provides properties and operations on the environment in which <see
/// cref="Activator.Activate{TResult}">activations</see> occur.
/// </summary>
public interface IActivationContext
{
    /// <summary>
    /// Indicates whether this <c>IActivationContext</c> contains data that is not intended for a production environment.
    /// </summary>
    public bool IsDevelopment { get; }
    /// <summary>
    /// Indicates whether this <c>IActivationContext</c> contains data that is intended for a testing environment.
    /// </summary>
    public bool IsExperimental { get; }
    
    /// <summary>
    /// Includes the specified <paramref name="dependency"/> in this <c>IActivationContext</c>.
    /// </summary>
    /// <typeparam name="TDependency">The <see cref="Type"/> of the specified <paramref name="dependency"/>.</typeparam>
    /// <param name="dependency">The object to include in this <c>IActivationContext</c>.</param>
    /// <returns>This <c>IActivationContext</c>.</returns>
    /// <exception cref="ArgumentException">If the <see cref="Type"/>s to which the specified <paramref name="dependency"/>
    /// belongs intersects with the types exhibited by another object in this <c>IActivationContext</c>.</exception>
    public IActivationContext AddDependency<TDependency>(TDependency dependency) where TDependency : notnull;
    /// <summary>
    /// Provides the dependency that exhibits the specified type, <typeparamref name="TDependency"/>, in this <c>IActivationContext</c>.
    /// </summary>
    /// <typeparam name="TDependency">The <see cref="Type"/> of the queried dependency.</typeparam>
    /// <returns>The dependency of the specified type, <typeparamref name="TDependency"/>.</returns>
    /// <exception cref="KeyNotFoundException">If there is no dependency with the specified type, <typeparamref name="TDependency"/>,
    /// in this <c>IActivationContext</c>.</exception>
    public TDependency GetDependency<TDependency>();
    /// <summary>
    /// Provides the dependency that exhibits the specified <paramref name="type"/> in this <c>IActivationContext</c>.
    /// </summary>
    /// <returns>The dependency that belongs to the specified <paramref name="type"/>.</returns>
    /// <exception cref="KeyNotFoundException">If there is no dependency with the specified <paramref name="type"/> in
    /// this <c>IActivationContext</c>.</exception>
    public object GetDependency(Type type);
    /// <summary>
    /// Associates the specified <paramref name="name"/> with the specified <paramref name="value"/> in this <c>IActivationContext</c>.
    /// </summary>
    /// <typeparam name="TValue">The <see cref="Type"/> over which the specified <paramref name="value"/> may range.</typeparam>
    /// <param name="name">The unique identifier for the specified <paramref name="value"/> in this <c>IActivationContext</c>.</param>
    /// <param name="value">The data to associate with the specified <paramref name="name"/> in this <c>IActivationContext</c>.</param>
    /// <param name="constant">If <c>true</c>, the association defined by the specified <paramref name="name"/> and
    /// <paramref name="value"/> may not be <see cref="Set{TValue}"> modified</see>.</param>
    /// <returns>This <c>IActivationContext</c>.</returns>
    /// <exception cref="ArgumentException">If the specified <paramref name="name"/> is already associated with another
    /// <paramref name="value"/> in this <c>IActivationContext</c>.</exception>
    public IActivationContext DefineVariable<TValue>(string name, TValue? value, bool constant = true);
    /// <summary>
    /// Provides the value associated with the specified <paramref name="name"/> in this <c>IActivationContext</c>.
    /// </summary>
    /// <typeparam name="TValue">The <see cref="Type"/> over which the value by the specified <paramref name="name"/> may
    /// range.</typeparam>
    /// <param name="name">The unique identifier for the queried value in this <c>IActivationContext</c>.</param>
    /// <returns>The value by the specified <paramref name="name"/> in this <c>IActivationContext</c>.</returns>
    /// <exception cref="KeyNotFoundException">If there is no value associated with the specified <paramref name="name"/>
    /// in this <c>IActivationContext</c>.</exception>
    public TValue? Get<TValue>(string name);
    /// <summary>
    /// Associates the specified <paramref name="name"/> with the specified <paramref name="value"/> in this <c>IActivationContext</c>,
    /// overwriting the previously associated data.
    /// </summary>
    /// /// <typeparam name="TValue">The <see cref="Type"/> over which the specified <paramref name="value"/> may range.</typeparam>
    /// <param name="name">The unique identifier for the specified <paramref name="value"/> in this <c>IActivationContext</c>.</param>
    /// <param name="value">The data to associate with the specified <paramref name="name"/> in this <c>IActivationContext</c>.</param>
    /// <exception cref="InvalidOperationException">If the specified <paramref name="name"/> is associated with a constant
    /// <paramref name="value"/>.</exception>
    /// <exception cref="KeyNotFoundException">If the specified <paramref name="name"/> is not associated with a <paramref
    /// name="value"/> in this <c>IActivationContext</c>.</exception>
    public TValue? Set<TValue>(string name, TValue? value);
}

/// <summary>
/// The <c>ActivationContext</c> class provides a concrete implementation of the <see cref="IActivationContext"/>
/// interface.
/// </summary>
public sealed class ActivationContext : IActivationContext
{
    /// <summary>
    /// Provides a new <see cref="IActivationContext"/> in its native state.
    /// </summary>
    /// <param name="development">Indicates whether the new <c>IActivationContext</c> contains data that is not intended
    /// for a production environment.</param>
    /// <param name="experimental">Indicates whether the new <c>IActivationContext</c> contains data that is intended for
    /// a testing environment.</param>
    /// <returns>A new <see cref="IActivationContext"/>.</returns>
    public static IActivationContext Init(bool development = false, bool experimental = false)
    {
        return new ActivationContext(development, experimental);
    }
    
    /// <inheritdoc />
    public bool IsDevelopment { get; }

    /// <inheritdoc />
    public bool IsExperimental { get; }

    private readonly Dictionary<Type, object> _types = [];
    private readonly Dictionary<string, Variable> _names = [];

    private ActivationContext(bool development, bool experimental)
    {
        IsDevelopment = development;
        IsExperimental = experimental;
        foreach (var variable in GetVariables())
        {
            _names.Add(variable.Name, variable);
        }
    }
    
    private IEnumerable<Variable> GetVariables()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        return assemblies
               .SelectMany(MakeVariablesFromTypes)
               .Concat(assemblies.SelectMany(MakeVariablesFromFields))
               .ThrowIfDuplicatesBy(variable => variable.Name);
    }

    private IEnumerable<Variable> MakeVariablesFromTypes(Assembly assembly)
    {
        return assembly
               .GetTypes()
               .Where(IsTarget)
               .SelectMany(MakeVariables);
    }

    private IEnumerable<Variable> MakeVariablesFromFields(Assembly assembly)
    {
        return assembly
               .GetTypes()
               .SelectMany(type => type.GetFields())
               .Where(IsTarget)
               .SelectNotNull(MakeVariable);
    }

    private static bool IsTarget(Type type)
    {
        return type.IsPublic && type.IsStatic() && type.HasCustomAttribute<ActivationConfigurationAttribute>();
    }

    private static bool IsTarget(FieldInfo field)
    {
        return field.IsStatic && field.HasCustomAttribute<ActivationVariableAttribute>();
    }

    private IEnumerable<Variable> MakeVariables(Type type)
    {
        return type
               .GetCustomAttribute<ActivationConfigurationAttribute>()!
               .MakeVariables(type, IsDevelopment);
    }

    private Variable? MakeVariable(FieldInfo field)
    {
        var attribute = field.GetCustomAttribute<ActivationVariableAttribute>()!;
        if (!attribute.IsDevelopment || IsDevelopment)
        {
            return new Variable(attribute.Name, field.GetValue(null), attribute.IsConstant);
        }
        return null;
    }

    /// <inheritdoc />
    public IActivationContext AddDependency<TDependency>(TDependency dependency) where TDependency : notnull
    {
        foreach (var type in dependency.GetEveryType())
        {
            _types.Add(type, dependency);
        }
        return this;
    }

    /// <inheritdoc />
    public TDependency GetDependency<TDependency>()
    {
        return (TDependency)GetDependency(typeof(TDependency));
    }

    /// <inheritdoc />
    public object GetDependency(Type type)
    {
        return _types[type];
    }

    /// <inheritdoc />
    public IActivationContext DefineVariable<TValue>(string name, TValue? value, bool constant = false)
    {
        _names.Add(Guard.Against.NullOrWhitespace(name, nameof(name)), new Variable(name, value, constant));
        return this;
    }

    /// <inheritdoc />
    public TValue? Get<TValue>(string name)
    {
        return (TValue?)_names[name].Value;
    }

    /// <inheritdoc />
    public TValue? Set<TValue>(string name, TValue? value)
    {
        if (_names[name].IsConstant)
        {
            throw new InvalidOperationException();
        }
        _names[name] = new Variable(name, value, false);
        return value;
    }
}
