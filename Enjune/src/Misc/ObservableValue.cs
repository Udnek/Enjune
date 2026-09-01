namespace Enjune.Misc;

public delegate void ChangeEvent<in T>(T oldValue, T newValue);

public interface IReadonlyObservableValue<T>
{
    public event ChangeEvent<T>? OnChange;
    
    public T Val { get; }
}

public sealed class ObservableValue<T>(T initialValue) : IReadonlyObservableValue<T>
{
    public event ChangeEvent<T>? OnChange;

    public T Val
    {
        get;
        set
        {
            var old = field;
            field = value;
            if (!EqualityComparer<T>.Default.Equals(old, value)) 
                OnChange?.Invoke(old, value);
        }
    } = initialValue;
    
    public static implicit operator ObservableValue<T>(T value) => new(value);
    public static implicit operator T(ObservableValue<T> remember) => remember.Val;
}