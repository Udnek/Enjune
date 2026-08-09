namespace Enjune.Misc;

public sealed class Remember<T>(T initialValue)
{
    public T? OldValue { get; private set; } = default;

    public T Val
    {
        get;
        set
        {
            OldValue = field;
            field = value;
        }
    } = initialValue;

    public bool Changed => !Equals(OldValue, Val);
    
    public static implicit operator Remember<T>(T value) => new(value);
    public static implicit operator T(Remember<T> remember) => remember.Val;
}