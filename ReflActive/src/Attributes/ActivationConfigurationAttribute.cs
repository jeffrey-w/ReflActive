using System.Reflection;
using System.Text;
using Extensions;

namespace ReflActive.Attributes;

/// <summary>
/// The <c>ActivationConfigurationAttribute</c> identifies classes that define one or more values to include in the current
/// <see cref="IActivationContext"/>.
/// </summary>
/// <remarks>Only public, static values defined in public, static classes annotated by this <see cref="Attribute"/> are
/// included in the current <see cref="IActivationContext"/>. Values are associated with the name of the variable to which
/// they are assigned, and it is imperative that identifiers be unique. If two values are associated with the same name,
/// an <see cref="InvalidOperationException"/> is thrown.</remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ActivationConfigurationAttribute : Attribute
{
    /// <summary>
    /// The <c>ConstantAttribute</c> indicates that a value declared in a class exhibiting the <see cref="ActivationConfigurationAttribute"/>
    /// may not be changed once it is defined in the current <see cref="IActivationContext"/>.
    /// </summary>
    public sealed class ConstantAttribute : Attribute;
    
    /// <summary>
    /// Indicates that the identifiers associated with the values defined in the class annotated by this
    /// <c>ActivationConfigurationAttribute</c> will have a space inserted before each word prior to being associated with
    /// those values in the current <see cref="IActivationContext"/>.
    /// </summary>s
    public bool UseWhitespaceInNames { get; init; } = false;
    /// <summary>
    /// Indicates whether this <c>ActivationConfigurationAttribute</c> annotates a class that defines values that are not
    /// intended for production.
    /// </summary>
    public bool IsDevelopment { get; init; } = false;

    internal IEnumerable<Variable> MakeVariables(Type type, bool development)
    {
       return type
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .WhereNot(field => field.HasCustomAttribute<ActivationVariableAttribute>())
            .SelectNotNull(field => MakeVariable(field, development));
    }

    private Variable? MakeVariable(FieldInfo field, bool development)
    {
        if (!IsDevelopment || development)
        {
            return new Variable(GetName(field), field.GetValue(null), field.HasCustomAttribute<ConstantAttribute>());
        }
        return null;
    }

    private string GetName(FieldInfo field)
    {
        var name = field.Name;
        if (UseWhitespaceInNames)
        {
            return name
                .Skip(1)
                .Aggregate(
                    new StringBuilder($"{name.First()}"), 
                    (builder, c) => char.IsUpper(c) ? builder.Append($" {c}") : builder.Append(c),
                    builder => builder.ToString());
        }
        return name;
    }
}