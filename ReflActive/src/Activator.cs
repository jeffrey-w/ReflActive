using System.Reflection;
using Extra.Extensions;
using Extra.Guard;
using ReflActive.Attributes;

namespace ReflActive;

/// <summary>
/// The <c>Activator</c> class provides a facility for instantiating <see cref="Type"/>s.
/// </summary>
public static class Activator
{
    /// <summary>
    /// Provides the member of <typeparamref name="TResult"/> targeted by the specified <paramref name="activation"/>.
    /// </summary>
    /// <typeparam name="TResult">The type being instantiated.</typeparam>
    /// <param name="activation">An invocation of the constructor for the targeted member of <typeparamref name="TResult"/>.</param>
    /// <param name="context">The current <see cref="IActivationContext"/>.</param>
    /// <returns>The targeted member of <typeparamref name="TResult"/>.</returns>
    /// <exception cref="ArgumentException">If any pair of parameters to the constructor targeted by the specified
    /// <paramref name="activation"/> exhibits the same <see cref="ParameterAttribute.Name"/>, or if the
    /// <see cref="Activation.Arguments"/> associtaed with the specified <paramref name="activation"/> do not belong to
    /// the same <see cref="Type"/>s as the parameters they target.</exception>
    /// <exception cref="InvalidOperationException">If the specified <paramref name="activation"/> does not target a
    /// constructor for a member of <typeparamref name="TResult"/>, if there is not exactly one member of
    /// <typeparamref name="TResult"/> that exhibits the same <see cref="ActivationTargetAttribute.Name"/> and
    /// <see cref="ActivationTargetAttribute.Discriminator"/> as the specified <paramref name="activation"/>, if it does
    /// not declare exactly one constructor that exhibits the <see cref="ActivationTargetConstructorAttribute"/>, if any
    /// of them do not exhibit either the <see cref="ParameterAttribute"/> or the <see cref="DependencyAttribute"/>, or
    /// if any parameter it declares is subject to an <see cref="IEntityConverter"/> that does not declare a constructor
    /// parameterized only by the current <see cref="IActivationContext"/>.</exception>
    /// <exception cref="KeyNotFoundException">If the constructor targeted by the specified <paramref name="activation"/>
    /// declares a parameter that is neither <see cref="ParameterAttribute.Name">named</see> nor belongs to a <see
    /// cref="Type"/> for which a <see cref="IActivationContext.GetDependency">dependency</see> exists in the current
    /// <see cref="IActivationContext"/>.</exception>
    /// <exception cref="MemberAccessException">If the constructor targeted by the specified <paramref name="activation"/>
    /// is declared on an abstract class.</exception>
    /// <exception cref="MethodAccessException">If the constructor targeted by the specified <paramref name="activation"/>
    /// is private or protected, and the caller lacks <see cref="F:System.Security.Permissions.ReflectionPermissionFlag.MemberAccess" />.</exception>
    /// <exception cref="TargetInvocationException">If the constructor targeted by one of the specified <paramref name="activation"/>
    /// throws an exception.</exception>
    /// <exception cref="TargetParameterCountException">If the <see cref="Activation.Arguments"/> assocaited with the
    ///  specified <paramref name="activation"/> do not satisfy the arity of the constructor they target.</exception>
    public static TResult Activate<TResult>(Activation activation, IActivationContext context)
    {
        var constructor = GetConstructor<TResult>(activation);
        return (TResult)constructor.Invoke(GetValues(constructor, activation.ToBindings(), context));
    }

    private static ConstructorInfo GetConstructor<TResult>(Activation activation)
    {
        return EveryType
               .That
               .IsAssignableTo<TResult>()
               .Single(type => IsTargetedType(type, activation))
               .GetConstructors()
               .Single(info => info.HasCustomAttribute<ActivationTargetConstructorAttribute>());
    }

    private static bool IsTargetedType(Type type, Activation activation)
    {
        // TODO verify that type is valid for context
        return type
            .GetCustomAttribute<ActivationTargetAttribute>()!
            .IsTargetedBy(activation);
    }
    
    private static object?[] GetValues(
        ConstructorInfo constructor, Dictionary<string, object?> bindings, IActivationContext context)
    {
        return Against
               .Violation(
                   constructor
                       .GetParameters(),
                   HaveUniqueNames)
               .Select(parameter => GetValue(parameter, bindings, context))
               .ToArray();
    }

    private static bool HaveUniqueNames(ParameterInfo[] parameters)
    {
        return parameters
               .Where(parameter => parameter.HasCustomAttribute<ParameterAttribute>())
               .SelectMany((parameter, i) => GetPairsFor(i, parameter, parameters))
               .NotAny(NamesEqual);
    }

    private static IEnumerable<(ParameterInfo, ParameterInfo)> GetPairsFor(
        int i,
        ParameterInfo parameter,
        ParameterInfo[] parameters)
    {
        return parameters
               .Where((_, j) => j > i)
               .Select(second => (parameter, second));
    }

    private static bool NamesEqual((ParameterInfo first, ParameterInfo second) pair)
    {
        return pair.first.GetCustomAttribute<ParameterAttribute>()!.Name ==
               pair.second.GetCustomAttribute<ParameterAttribute>()!.Name;
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
            _ => value,
        };
    }

    /// <summary>
    /// Provides every <see cref="ActivationTargetAttribute.IsPermanent">permanent</see> member of <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type being instantiated.</typeparam>
    /// <param name="context">The current <see cref="IActivationContext"/>.</param>
    /// <returns>Every member of <typeparamref name="TResult"/> that is<see cref="ActivationTargetAttribute.IsPermanent">permanent</see>.</returns>
    /// <exception cref="InvalidOperationException">If any of the <see cref="ActivationTargetAttribute.IsPermanent">permanent</see>
    /// members of <typeparamref name="TResult"/> declare neither a default constructor nor one that is only parameterized
    /// by the current <see cref="IActivationContext"/>.</exception>
    public static IEnumerable<TResult> ActivatePermanentTargets<TResult>(IActivationContext context)
    {
        return EveryType
               .That
               .IsAssignableTo<TResult>()
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
