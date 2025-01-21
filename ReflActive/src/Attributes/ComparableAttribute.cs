namespace ReflActive.Attributes;

/// <summary>
/// The <c>ComparableAttribute</c> annotates parameters to a constructor exhibiting the <see cref="ActivationTargetConstructorAttribute"/>
/// that exhibit a well-defined order and minimal and maximal elements. 
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public abstract class ComparableAttribute<TComparable> : Attribute where TComparable : IComparable<TComparable>
{
    private readonly object _min;
    private readonly object _max;

    /// <summary>
    /// Creates a new <c>ComparableAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The lowest value that the parameter annotated by the new <c>ComparableAttribute</c> may hold.</param>
    /// <param name="max">The highest value that the parameter annotated by the new <c>ComparableAttribute</c> may hold.</param>
    protected ComparableAttribute(TComparable min, TComparable max)
    {
        _min = min;
        _max = max;
    }

    /// <summary>
    /// Creates a new <c>ComparableAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The lowest value that the parameter annotated by the new <c>ComparableAttribute</c> may hold.</param>
    /// <param name="max">The unique identifier for the highest value that the parameter annotated by the new <c>ComparableAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <exception cref="KeyNotFoundException">If there is no value associated with the specified <paramref name="max"/>
    /// in the current <see cref="IActivationContext"/>.</exception>
    /// <exception cref="InvalidCastException">If the value associated with the specified <paramref name="max"/> in
    /// the current <see cref="IActivationContext"/> does not belong to <typeparamref name="TComparable"/>.</exception>
    protected ComparableAttribute(TComparable min, string max)
    {
        _min = min;
        _max = max;
    }

    /// <summary>
    /// Creates a new <c>ComparableAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The unique identifier for the lowest value that the parameter annotated by the new <c>ComparableAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <param name="max">The highest value that the parameter annotated by the new <c>ComparableAttribute</c> may hold.</param>
    /// <exception cref="KeyNotFoundException">If there is no value associated with the specified <paramref name="min"/>
    /// in the current <see cref="IActivationContext"/>.</exception>
    /// <exception cref="InvalidCastException">If the value associated with the specified <paramref name="min"/> in
    /// the current <see cref="IActivationContext"/> does not belong to <typeparamref name="TComparable"/>.</exception>
    protected ComparableAttribute(string min, TComparable max)
    {
        _min = min;
        _max = max;
    }

    /// <summary>
    /// Creates a new <c>ComparableAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The unique identifier for the lowest value that the parameter annotated by the new <c>ComparableAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <param name="max">The unique identifier for the highest value that the parameter annotated by the new <c>ComparableAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <exception cref="KeyNotFoundException">If there is no value associated with the specified <paramref name="min"/>
    /// or <paramref name="max"/> in the current <see cref="IActivationContext"/>.</exception>
    /// <exception cref="InvalidCastException">If the value associated with the specified <paramref name="min"/> or
    /// <paramref name="max"/> in the current <see cref="IActivationContext"/> does not belong to <typeparamref name="TComparable"/>.</exception>
    protected ComparableAttribute(string min, string max)
    {
        _min = min;
        _max = max;
    }
    
    internal TComparable GetMin(IActivationContext context)
    {
        return GetValue(_min, context);
    }

    internal TComparable GetMax(IActivationContext context)
    {
        return GetValue(_max, context);
    }
    
    private static TComparable GetValue(object value, IActivationContext context)
    {
        return value is TComparable c ? c : context.Get<TComparable>((string)value) ?? throw new InvalidOperationException();
    }
}

/// <summary>
/// The <c>DiscreteNumberAttribute</c> annotates parameters to a constructor exhibiting the <see cref="ActivationTargetConstructorAttribute"/>
/// that ranges over a subset of the integers.
/// </summary>
public sealed class DiscreteNumberAttribute : ComparableAttribute<int>
{
    /// <summary>
    /// The interval between successive elements of the domain of this <c>IDiscreteNumberParameter</c>. 
    /// </summary>
    public int Step { get; init; } = 1;
    
    /// <summary>
    /// Creates a new <c>DiscreteNumberAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The lowest value that the parameter annotated by the new <c>DiscreteNumberAttribute</c> may
    /// hold.</param>
    /// <param name="max">The highest value that the parameter annotated by the new <c>DiscreteNumberAttribute</c> may
    /// hold.</param>
    public DiscreteNumberAttribute(int min, int max) : base(min, max)
    {
    }

    /// <summary>
    /// Creates a new <c>DiscreteNumberAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The lowest value that the parameter annotated by the new <c>DiscreteNumberAttribute</c> may
    /// hold.</param>
    /// <param name="max">The unique identifier for the highest value that the parameter annotated by the new <c>DiscreteNumberAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <exception cref="KeyNotFoundException">If there is no value associated with the specified <paramref name="max"/>
    /// in the current <see cref="IActivationContext"/>.</exception>
    /// <exception cref="InvalidCastException">If the value associated with the specified <paramref name="max"/> in the
    /// current <see cref="IActivationContext"/> is not an <see cref="int"/>.</exception>
    public DiscreteNumberAttribute(int min, string max) : base(min, max)
    {
    }

    /// <summary>
    /// Creates a new <c>DiscreteNumberAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The unique identifier for the lowest value that the parameter annotated by the new <c>DiscreteNumberAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <param name="max">The highest value that the parameter annotated by the new <c>DiscreteNumberAttribute</c> may
    /// hold.</param>
    /// <exception cref="KeyNotFoundException">If there is no value associated with the specified <paramref name="min"/>
    /// in the current <see cref="IActivationContext"/>.</exception>
    /// <exception cref="InvalidCastException">If the value associated with the specified <paramref name="min"/> in the
    /// current <see cref="IActivationContext"/> is not an <see cref="int"/>.</exception>
    public DiscreteNumberAttribute(string min, int max) : base(min, max)
    {
    }

    /// <summary>
    /// Creates a new <c>DiscreteNumberAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The unique identifier for the lowest value that the parameter annotated by the new <c>DiscreteNumberAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <param name="max">The unique identifier for the highest value that the parameter annotated by the new <c>DiscreteNumberAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <exception cref="KeyNotFoundException">If there is no value associated with the specified <paramref name="min"/>
    /// or <paramref name="max"/> in the current <see cref="IActivationContext"/>.</exception>
    /// <exception cref="InvalidCastException">If the value associated with the specified <paramref name="min"/> or
    /// <paramref name="max"/> in the current <see cref="IActivationContext"/> is not an <see cref="int"/>.</exception>
    public DiscreteNumberAttribute(string min, string max) : base(min, max)
    {
    }
}

/// <summary>
/// The <c>DiscreteNumberAttribute</c> annotates parameters to a constructor exhibiting the <see cref="ActivationTargetConstructorAttribute"/>
/// that ranges over a subset of the real numbers.
/// </summary>
public sealed class ContinuousNumberAttribute : ComparableAttribute<double>
{
    /// <summary>
    /// The degree by which successive elements of the domain of the parameter annotated by this <c>ContinuousNumberAttribute</c>
    /// vary.
    /// </summary>
    public int Precision { get; init; } = 3;
    
    /// <summary>
    /// Creates a new <c>ContinuousNumberAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The lowest value that the parameter annotated by the new <c>ContinuousNumberAttribute</c> may
    /// hold.</param>
    /// <param name="max">The highest value that the parameter annotated by the new <c>ContinuousNumberAttribute</c> may
    /// hold.</param>
    public ContinuousNumberAttribute(double min, double max) : base(min, max)
    {
    }

    /// <summary>
    /// Creates a new <c>ContinuousNumberAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The lowest value that the parameter annotated by the new <c>ContinuousNumberAttribute</c> may
    /// hold.</param>
    /// <param name="max">The unique identifier for the highest value that the parameter annotated by the new <c>ContinuousNumberAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <exception cref="KeyNotFoundException">If there is no value associated with the specified <paramref name="max"/>
    /// in the current <see cref="IActivationContext"/>.</exception>
    /// <exception cref="InvalidCastException">If the value associated with the specified <paramref name="max"/> in the
    /// current <see cref="IActivationContext"/> is not an <see cref="double"/>.</exception>
    public ContinuousNumberAttribute(double min, string max) : base(min, max)
    {
    }

    /// <summary>
    /// Creates a new <c>ContinuousNumberAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The unique identifier for the lowest value that the parameter annotated by the new <c>ContinuousNumberAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <param name="max">The highest value that the parameter annotated by the new <c>ContinuousNumberAttribute</c> may
    /// hold.</param>
    /// <exception cref="KeyNotFoundException">If there is no value associated with the specified <paramref name="min"/>
    /// in the current <see cref="IActivationContext"/>.</exception>
    /// <exception cref="InvalidCastException">If the value associated with the specified <paramref name="min"/> in the
    /// current <see cref="IActivationContext"/> is not an <see cref="double"/>.</exception>
    public ContinuousNumberAttribute(string min, double max) : base(min, max)
    {
    }

    /// <summary>
    /// Creates a new <c>ContinuousNumberAttribute</c> with the specified <paramref name="min"/> and <paramref name="max"/>
    /// values.
    /// </summary>
    /// <param name="min">The unique identifier for the lowest value that the parameter annotated by the new <c>ContinuousNumberAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <param name="max">The unique identifier for the highest value that the parameter annotated by the new <c>ContinuousNumberAttribute</c>
    /// may hold in the current <see cref="IActivationContext"/>.</param>
    /// <exception cref="KeyNotFoundException">If there is no value associated with the specified <paramref name="min"/>
    /// or <paramref name="max"/> in the current <see cref="IActivationContext"/>.</exception>
    /// <exception cref="InvalidCastException">If the value associated with the specified <paramref name="min"/> or
    /// <paramref name="max"/> in the current <see cref="IActivationContext"/> is not an <see cref="double"/>.</exception>
    public ContinuousNumberAttribute(string min, string max) : base(min, max)
    {
    }
}
