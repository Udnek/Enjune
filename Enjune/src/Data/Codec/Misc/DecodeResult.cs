namespace Enjune.Data.Codec.Misc;

public readonly record struct DecodeResult<T>
{
    private readonly T _value;
    public readonly Error? Error;

    internal DecodeResult(T value, Error? error)
    {
        _value = value;
        Error = error;
    }

    public T GetOr(T fallback) => Error == null ? _value : fallback;
    public T GetOrThrow() => Error == null ? _value : throw new InvalidOperationException();
    
    public static implicit operator DecodeResult<T>(Error error) => DecodeResult.Failure<T>(error);
}

public static class DecodeResult
{
    public static DecodeResult<T?> ToNullable<T>(DecodeResult<T> res) where T : class
    {
        if (res.Error != null)
            return (Error)res.Error;
        return Success<T?>(res.GetOrThrow());
    }
    public static DecodeResult<T?> ToNullableStruct<T>(DecodeResult<T> res) where T : struct
    {
        if (res.Error != null)
            return (Error)res.Error;
        return Success<T?>(res.GetOrThrow());
    }
    public static DecodeResult<T> Success<T>(T value) => new(value, null);
    public static DecodeResult<T> Failure<T>(Error err) => new(default!, err);
}