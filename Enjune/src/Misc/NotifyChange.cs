namespace Enjune.Misc;

public delegate void ChangeEvent<in T>(T oldValue, T newValue);

public interface IReadonlyNotifyChange<T>
{
    public event ChangeEvent<T>? OnChange;
    
    public T Val { get; }
}

public sealed class NotifyChange<T>(T initialValue) : IReadonlyNotifyChange<T>
{
    public event ChangeEvent<T>? OnChange;

    public T Val
    {
        get;
        set
        {
            var old = field;
            field = value;
            if (!Equals(old, value)) 
                OnChange?.Invoke(old, value);
        }
    } = initialValue;
    
    public static implicit operator NotifyChange<T>(T value) => new(value);
    public static implicit operator T(NotifyChange<T> remember) => remember.Val;
}