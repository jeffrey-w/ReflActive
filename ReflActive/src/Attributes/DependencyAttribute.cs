using System.Reflection;

namespace ReflActive.Attributes;

/// <summary>
/// The <c>DependencyAttribute</c> annotates parameters to a constructor that exhibits the <see cref="ActivationTargetConstructorAttribute"/>
/// that may be bound to any member of a <see cref="Type"/> for which a <see cref="IActivationContext.GetDependency">
/// dependency</see> exists.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class DependencyAttribute : Attribute
{
    internal object GetValue(ParameterInfo parameter, IActivationContext context)
    {
        return context.GetDependency(parameter.ParameterType);
    }
}