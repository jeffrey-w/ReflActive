using System.Reflection;
using Extensions;
using ReflActive.Attributes;

namespace ReflActive;

/// <summary>
/// The <c>Activator</c> class provides a facility for instantiating <see cref="Type"/>s.
/// </summary>
public static class Activator
{
    /// <summary>
    /// Provides the members of <typeparamref name="TResult"/> targeted by the specified <paramref name="activations"/>.
    /// </summary>
    /// <param name="activations">Invocations of the constructors for the targeted members of <typeparamref name="TResult"/>.</param>
    /// <param name="context">The current <see cref="IActivationContext"/>.</param>
    /// <returns>The targeted members of <typeparamref name="TResult"/>.</returns>
    /// <exception cref="ArgumentException">If any pair of parameters to the constructor targeted by one of the specified
    /// <paramref name="activations"/> exhibits the same <see cref="ParameterAttribute.Name"/>, if any of them does not
    /// exhibit either the <see cref="ParameterAttribute"/> or the <see cref="DependencyAttribute"/>, or if the <see
    /// cref="Activation.Arguments"/> associtaed with any of the specified <paramref name="activations"/> do not belong
    /// to the same <see cref="Type"/>s as the parameters they target.</exception>
    /// <exception cref="InvalidCastException">If any of the specified <paramref name="activations"/> do not target a
    /// constructor for a member of <typeparamref name="TResult"/>.</exception>
    /// <exception cref="InvalidOperationException">If there is not exactly one member of <typeparamref name="TResult"/>
    /// that exhibits the same <see cref="ActivationTargetAttribute.Name"/> and <see cref="ActivationTargetAttribute.Discriminator"/>
    /// for each of the specified <paramref name="activations"/>, if it does not declare exactly one constructor that
    /// exhibits the <see cref="ActivationTargetConstructorAttribute"/>, if any parameter it declares is subject to an <see
    /// cref="IEntityConverter"/> that does not declare a constructor that is parameterized only by the current <see
    /// cref="IActivationContext"/>, or if a <see cref="ActivationTargetAttribute.IsPermanent">permanent</see> member of
    /// <typeparamref name="TResult"/> does not declare either a default constructor or one that is only parameterized by
    /// the current <see cref="IActivationContext"/>.</exception>
    /// <exception cref="KeyNotFoundException">If any constructor targeted by one of the specified <paramref name="activations"/>
    /// declares a parameter that is not either <see cref="ParameterAttribute.Name">named</see> or belongs to a <see
    /// cref="Type"/> for which no <see cref="IActivationContext.GetDependency">dependency</see> exists.</exception>
    /// <exception cref="MemberAccessException">If any constructor targeted by one of the specified <paramref name="activations"/>
    /// is declared on an abstract class.</exception>
    /// <exception cref="MethodAccessException">If any constructor targeted by the specified <paramref name="activations"/>
    /// is private or protected, and the caller lacks <see cref="F:System.Security.Permissions.ReflectionPermissionFlag.MemberAccess" />.</exception>
    /// <exception cref="TargetInvocationException">If any constructor targeted by one of the specified <paramref name="activations"/>
    /// throws an exception.</exception>
    /// <exception cref="TargetParameterCountException">If the <see cref="Activation.Arguments"/> assocaited with any of
    /// the specified <paramref name="activations"/> do not satisfy the arity of the constructor they target.</exception>
    public static IEnumerable<TResult> Activate<TResult, TAttribute>(
        IEnumerable<Activation> activations, IActivationContext context) where TAttribute : ActivationTargetAttribute
    {
        return activations
            .Select(activation => Activate<TAttribute>(activation, context))
            .Cast<TResult>()
            .Concat(ActivatePermanentTargets<TResult, TAttribute>(context));
    }
    
    private static object Activate<TAttribute>(
        Activation activation, IActivationContext context) where TAttribute : ActivationTargetAttribute
    {
        var constructor = GetConstructor<TAttribute>(activation);
        return constructor.Invoke(GetValues(constructor, activation.ToBindings(), context));
    }

    private static ConstructorInfo GetConstructor<TAttribute>(
        Activation activation) where TAttribute : ActivationTargetAttribute
    {
        return EveryType.That
            .HasAttribute<TAttribute>()
            .Single(type => IsTargetedType<TAttribute>(type, activation))
            .GetConstructors()
            .Single(info => info.HasCustomAttribute<ActivationTargetConstructorAttribute>());
    }

    private static bool IsTargetedType<TAttribute>(
        Type type, Activation activation) where TAttribute : ActivationTargetAttribute
    {
        return type
            .GetCustomAttribute<TAttribute>()!
            .IsTargetedBy(activation);
    }
    
    private static object?[] GetValues(
        ConstructorInfo constructor, Dictionary<string, object?> bindings, IActivationContext context)
    {
        return constructor
            .GetParameters()
            .Select(parameter => GetValue(parameter, bindings, context))
            .ToArray();
    }

    private static object? GetValue(ParameterInfo parameter, Dictionary<string, object?> bindings, IActivationContext context)
    {
        if (parameter.HasCustomAttribute<ParameterAttribute>())
        {
            return GetEntitiesOrDefault(parameter, bindings[GetNameFor(parameter)], context);
        }

        if (parameter.HasCustomAttribute<DependencyAttribute>())
        {
            return parameter
                .GetCustomAttribute<DependencyAttribute>()!
                .GetValue(parameter, context);
        }
        throw new InvalidOperationException();
    }

    private static string GetNameFor(ParameterInfo parameter)
    {
        return parameter
            .GetCustomAttribute<ParameterAttribute>()!
            .Name;
    }

    private static object? GetEntitiesOrDefault(ParameterInfo parameter, object? value, IActivationContext context)
    {
        return value switch
        {
            string id when parameter.HasCustomAttribute<SingletonEntityAttribute>() => 
                parameter
                    .GetCustomAttribute<SingletonEntityAttribute>()!
                    .GetEntity(id, context),
            IEnumerable<string> ids when parameter.HasCustomAttribute<CompositeEntityAttribute>() => 
                parameter
                    .GetCustomAttribute<CompositeEntityAttribute>()!
                    .GetEntities(ids, context),
            _ => value
        };
    }

    private static IEnumerable<TResult> 
        ActivatePermanentTargets<TResult, TAttribute>(IActivationContext context) where TAttribute : ActivationTargetAttribute
    {
        return EveryType.That
            .HasAttribute<TAttribute>()
            .Where(type => type
                .GetCustomAttribute<ActivationTargetAttribute>()!
                .IsPermanent)
            .SelectNotNull(type => (TResult)System.Activator.CreateInstance(type, GetArguments(type, context))!);
    }

    private static object?[] GetArguments(Type type, IActivationContext context)
    {
        return type
                .GetConstructors()
                .Single()
                .GetParameters()
                .IsSingleton()
            ? [context]
            : [];
    }
}
