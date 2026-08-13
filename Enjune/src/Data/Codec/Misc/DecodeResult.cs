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
    
    public Tto Map<Tto>(Func<T, Tto> whenSuccess, Func<Error, Tto> whenFailure) 
        => Error == null ? whenSuccess(_value) : whenFailure((Error)Error);

    public T GetOr(T fallback) => Error == null ? _value : fallback;
    
    // Use Map<Tto> in most cases
    public T GetOrThrow() => Error == null ? _value : throw new InvalidOperationException();
    
    public static implicit operator DecodeResult<T>(Error error) => DecodeResult.Failure<T>(error);
}

public static class DecodeResult
{
    public static DecodeResult<TNew> Convert<TOld, TNew>(DecodeResult<TOld> res) where TOld : TNew
    {
        return res.Map(
            value => Success<TNew>(value),
            Failure<TNew>);
    }
    
    public static DecodeResult<T> Success<T>(T value) => new(value, null);
    public static DecodeResult<T> Failure<T>(Error err) => new(default!, err);
}