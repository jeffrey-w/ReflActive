using System.Reflection;
using ReflActive.Attributes;

namespace ReflActive;

/// <summary>
/// The <c>IParameter</c> interface provides properties and operations on a variable data passed to a function.
/// </summary>
/// <typeparam name="TDomain">The set of values assignable to this <c>IParameter</c>.</typeparam>
public interface IParameter<out TDomain>
{
    /// <summary>
    /// The unique identifier for this <c>IParameter</c>.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The natural language characterization for this <c>IParameter</c>.
    /// </summary>
    public string Description { get; }
    /// <summary>
    /// Indicates whether this <c>IParameter</c> must be assigned a value.
    /// </summary>
    public bool IsRequired { get; }
    /// <summary>
    /// The typical value bound this <c>IParameter</c>.
    /// </summary>
    public TDomain? Default { get; }
}

/// <summary>
/// The <c>IBooleanParameter</c> interface provides properties and operation on an <see cref="IParameter{TDomain}"/>
/// that may be bound to a member of <see cref="bool"/>.
/// </summary>
public interface IBooleanParameter : IParameter<bool>;

/// <summary>
/// The <c>IComparableParameter</c> interface provides properties and operations on an <see cref="IParameter{TDomain}"/>,
/// the domain of which exhibit a well-defined order and minimal and maximal elements.
/// </summary>
/// <typeparam name="TDomain"></typeparam>
public interface IComparableParameter<out TDomain> : IParameter<TDomain> where TDomain : IComparable<TDomain>
{
    /// <summary>
    /// The lowest value that this <c>IComparableParameter</c> may hold.
    /// </summary>
    public TDomain Min { get; }
    /// <summary>
    /// The highest value that this <c>IComparableParameter</c> may hold.
    /// </summary>
    public TDomain Max { get; }
}

/// <summary>
/// The <c>IDiscreteNumberParameter</c> interface provides properties and operations on an <see cref="IComparableParameter{TDomain}"/>
/// that ranges over a subset of the integers.
/// </summary>
public interface IDiscreteNumberParameter : IComparableParameter<int>
{
    /// <summary>
    /// The interval between successive elements of the domain of this <c>IDiscreteNumberParameter</c>. 
    /// </summary>
    public int Step { get; }
}

/// <summary>
/// The <c>IDiscreteNumberParameter</c> interface provides properties and operations on an <see cref="IComparableParameter{TDomain}"/>
/// that ranges over a subset of the real numbers.
/// </summary>
public interface IContinuousNumberParameter : IComparableParameter<double>
{
    /// <summary>
    /// The degree by which successive elements of the domain of this <c>IContinuousNumberParameter</c> vary.
    /// </summary>
    public int Precision { get; }
}

/// <summary>
/// The <c>ITextParameter</c> interface provides properties and operations on an <see cref="IParameter{TDomain}"/> that
/// may be bound to a string.
/// </summary>
public interface ITextParameter : IParameter<string>
{
    /// <summary>
    /// The least number of characters the string bound to this <c>ITextParameter</c> may have.
    /// </summary>
    public int? Min { get; }
    /// <summary>
    /// The greatest number of characters the string bound to this <c>ITextParameter</c> may have.
    /// </summary>
    public int? Max { get; }
    /// <summary>
    /// The regular expression that defines the strings that may be bound to this <c>ITextParameter</c>.
    /// </summary>
    public string? Pattern { get; }
}

/// <summary>
/// The <c>ISingletonEntityParameter</c> interface provides properties and operations on an <see cref="IParameter{TDomain}"/>
/// that may be bound to a value from a finite subset of the strings that identify the members of a <see cref="Type"/>.
/// </summary>
public interface ISingletonEntityParameter : IParameter<string>
{
    /// <summary>
    /// The domain of this <c>ISingletonEntityParameter</c>.
    /// </summary>
    public IEnumerable<string> Values { get; }
}

/// <summary>
/// The <c>ICompositeEntityParameter</c> interface provides properties and operations on an <see cref="IParameter{TDomain}"/>
/// that may be simultaneously bound to multiple values from a finite subset of the strings that identify the members of
/// a <see cref="Type"/>.
/// </summary>
public interface ICompositeEntityParameter : IParameter<IEnumerable<string>>
{
    /// <summary>
    /// The domain of this <c>ICompositeEntityParameter</c>.
    /// </summary>
    public IEnumerable<string> Values { get; }
}

internal static class Parameter
{
    public sealed class Factory(IActivationContext context)
    {
        public IBooleanParameter MakeBoolean(ParameterInfo info)
        {
            return new BooleanParameter(info, context);
        }

        public IDiscreteNumberParameter MakeDiscreteNumber(ParameterInfo info)
        {
            return new DiscreteNumberParameter(info, context);
        }

        public IContinuousNumberParameter MakeContinuousNumber(ParameterInfo info)
        {
            return new ContinuousNumberParameter(info, context);
        }

        public ITextParameter MakeText(ParameterInfo info)
        {
            return new TextParameter(info, context);
        }

        public ISingletonEntityParameter MakeSingletonEntity(ParameterInfo info)
        {
            return new SingletonEntityParameter(info, context);
        }

        public ICompositeEntityParameter MakeCompositeEntity(ParameterInfo info)
        {
            return new CompositeEntityParameter(info, context);
        }
    }
    
    private abstract class BaseParameter<TDomain>(ParameterInfo info, IActivationContext context) : IParameter<TDomain>
    {
        public string Name => Info.GetCustomAttribute<ParameterAttribute>()?.Name ?? string.Empty;
        public string Description => Info.GetCustomAttribute<ParameterAttribute>()?.Description ?? string.Empty;
        public bool IsRequired => Nullable.GetUnderlyingType(Info.ParameterType) is null;
        public virtual TDomain? Default => (TDomain?)Info.GetCustomAttribute<DefaultAttribute>()?.GetValue(Context);

        protected ParameterInfo Info => info;
        protected IActivationContext Context => context;
    }

    private sealed class BooleanParameter(
        ParameterInfo info, IActivationContext context) : BaseParameter<bool>(info, context), IBooleanParameter;
    
    private sealed class DiscreteNumberParameter(
        ParameterInfo info, IActivationContext context) : BaseParameter<int>(info, context), IDiscreteNumberParameter
    {
        public int Min => Info.GetCustomAttribute<DiscreteNumberAttribute>()?.GetMin(Context) ?? int.MinValue;
        public int Max => Info.GetCustomAttribute<DiscreteNumberAttribute>()?.GetMax(Context) ?? int.MaxValue;
        public int Step => Info.GetCustomAttribute<DiscreteNumberAttribute>()?.Step ?? 1;
    }
    
    private sealed class ContinuousNumberParameter(
        ParameterInfo info, IActivationContext context) : BaseParameter<double>(info, context), IContinuousNumberParameter
    {
        public double Min => Info.GetCustomAttribute<ContinuousNumberAttribute>()?.GetMin(Context) ?? double.MinValue;
        public double Max => Info.GetCustomAttribute<ContinuousNumberAttribute>()?.GetMax(Context) ?? double.MaxValue;
        public int Precision => Info.GetCustomAttribute<ContinuousNumberAttribute>()?.Precision ?? 2;
    }
    
    private sealed class TextParameter(
        ParameterInfo info, IActivationContext context) : BaseParameter<string>(info, context), ITextParameter
    {
        public int? Min => Info.GetCustomAttribute<TextAttribute>()?.Min;
        public int? Max => Info.GetCustomAttribute<TextAttribute>()?.Max;
        public string? Pattern => Info.GetCustomAttribute<TextAttribute>()?.Pattern;
    }
    
    private sealed class SingletonEntityParameter(
        ParameterInfo info, IActivationContext context) : BaseParameter<string>(info, context), ISingletonEntityParameter
    {
        public IEnumerable<string> Values => Info.GetCustomAttribute<SingletonEntityAttribute>()?.GetIds(Context) ?? [];
        public override string Default =>
            Info.GetCustomAttribute<SingletonEntityAttribute>()?.GetDefault(Context) ?? string.Empty;
    }
    
    private sealed class CompositeEntityParameter(
        ParameterInfo info, IActivationContext context) : BaseParameter<IEnumerable<string>>(info, context), ICompositeEntityParameter
    {
        public IEnumerable<string> Values => Info.GetCustomAttribute<CompositeEntityAttribute>()?.GetIds(Context) ?? [];
        public override IEnumerable<string> Default =>
            Info.GetCustomAttribute<CompositeEntityAttribute>()?.GetDefaults(Context) ?? [];
    }
}
