using JetBrains.Annotations;

namespace Enjune.Misc;

public delegate void ChangeEvent<in T>(T oldValue, T newValue);

public interface IReadonlyObservableValue<out T>
{
    /// <summary>
    /// Subscribes to change. Unsubscriber should be used to unsubscribe and thus avoid memory leak
    /// </summary>
    /// <param name="action"></param>
    /// <returns>Unsubscriber to be called when action's owner is disposed</returns>
    [MustUseReturnValue]
    public Unsubscriber Observe(ChangeEvent<T> action);
    
    /// <summary>
    /// Current value
    /// </summary>
    public T Val { get; }
}

public sealed class ObservableValue<T>(T initialValue) : IReadonlyObservableValue<T>
{
    private event ChangeEvent<T>? OnChangeEvent;

    /// <summary>
    /// Subscribes to change. But may cause memory leak, so use it only if delegate is disposed with value's owner
    /// </summary>
    /// <param name="action"></param>
    public void ObserveAsOwner(ChangeEvent<T> action)
    {
        OnChangeEvent += action;
    }
    
    public Unsubscriber Observe(ChangeEvent<T> action)
    {
        OnChangeEvent += action;
        return new Unsubscriber(() => OnChangeEvent -= action);
    }

    public T Val
    {
        get;
        set
        {
            var old = field;
            field = value;
            if (!EqualityComparer<T>.Default.Equals(old, value)) 
                OnChangeEvent?.Invoke(old, value);
        }
    } = initialValue;

    public override string ToString() => $"{Logger.GetTypeName<ObservableValue<T>>()}[value = {Val?.ToString()}]";

    public static implicit operator ObservableValue<T>(T value) => new(value);
    public static implicit operator T(ObservableValue<T> observable) => observable.Val;
}