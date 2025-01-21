namespace ReflActive.Attributes;

/// <summary>
/// The <c>ActivationTargetConstructorAttribute</c> annotates constructors that are invoked by the <see cref="Activator"/>.
/// </summary>
/// <remarks>It is advised that only one constructor for a <see cref="Type"/> exhibits this <see cref="Attribute"/>. If
/// that condition is not met, and the <see cref="IParameter{TDomain}"/>s described by the <see cref="IActivationTargetMetadata"/>
/// for the <see cref="Type"/> are queried, then an <see cref="InvalidOperationException"/> will be thrown.</remarks>
[AttributeUsage(AttributeTargets.Constructor)]
public sealed class ActivationTargetConstructorAttribute : Attribute;
