using System.Reflection;
using ReflActive.Attributes;

namespace ReflActive;

/// <summary>
/// The <c>NameResolver</c> class provides a means for obtaining the identifiers for <see cref="Type"/>s that exhibit the
/// <see cref="ActivationTargetAttribute"/>.
/// </summary>
public static class NameResolver
{
    /// <summary>
    /// Provides the identifier for the specified <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The object whose name is being queried.</param>
    /// <returns>The name of the specified <paramref name="target"/>.</returns>
    /// <exception cref="ArgumentException">If the specified <paramref name="target"/> does not exhibit the
    /// <see cref="ActivationTargetAttribute"/>.</exception>
    public static string Resolve(object target)
    {
        return Resolve(target.GetType());
    }

    /// <summary>
    /// Provides the identifier for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> whose name is being queried.</param>
    /// <returns>The name of the specified <paramref name="type"/>.</returns>
    /// <exception cref="ArgumentException">If the specified <paramref name="type"/> does not exhibit the
    /// <see cref="ActivationTargetAttribute"/>.</exception>
    public static string Resolve(Type type)
    {
        return GetNameFrom(
            type.GetCustomAttribute<ActivationTargetAttribute>() ??
            throw new ArgumentException("The provided argument does not declare the ActivationTargetAttribute"));
    }
    
    private static string GetNameFrom(ActivationTargetAttribute attribute)
    {
        return $"{attribute.Name}{GetSuffix(attribute)}";
    }

    private static string GetSuffix(ActivationTargetAttribute attribute)
    {
        return string.IsNullOrWhiteSpace(attribute.Discriminator)
            ? string.Empty
            : $" - {attribute.Discriminator.ToLower()}";
    }
}
