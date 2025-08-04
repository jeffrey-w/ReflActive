using System.Collections.Immutable;
using Extensions;

namespace ReflActive.Attributes;

/// <summary>
/// The <c>PropertiesAttribute</c> annotates <see cref="Type"/>s with additional descriptive information.
/// </summary>
/// <param name="target">The class that defines additional descriptive attributes for the <see cref="Type"/> annotated by
/// the new <c>PropertiesAttribute</c>.</param>
/// <param name="args">Optional arguments to the constructor for the <see cref="TargetType"/> associated with this
/// <c>PropertiesAttribute</c></param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class PropertiesAttribute(Type target, params string[] args) : Attribute
{
    /// <summary>
    /// The class that defines additional descriptive attributes for the <see cref="Type"/> annotated by this <c>PropertiesAttribute</c>.
    /// </summary>
    /// <remarks>
    /// The <see cref="Type"/> assigned to this property must be either a static class, or one that declares a default
    /// constructor.
    /// </remarks>
    public Type TargetType => target;

    private object? Instance =>
        TargetType.IsStatic()
            ? null
            : TargetType
              .GetConstructor(
                  Enumerable
                      .Repeat(typeof(string), args.Length)
                      .ToArray())!
              .Invoke(
                  args
                      .Cast<object?>()
                      .ToArray());
    
    internal ImmutableDictionary<string, string> Compile(HashSet<string> exclude)
    {
        var instance = Instance;
        return TargetType
               .GetProperties()
               .Where(property => property.CanRead && !exclude.Contains(property.Name))
               .ToImmutableDictionary(
                   property => property.Name,
                   property => property.GetMethod!
                                       .Invoke(instance, [])
                                       ?.ToString() ??
                               string.Empty);
    }
}
