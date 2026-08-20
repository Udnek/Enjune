using System.Diagnostics.Contracts;

namespace Enjune.Misc;

public readonly record struct ResultOrError<T>
{
    private readonly T _value;
    public readonly Error? Error;

    internal ResultOrError(T value, Error? error)
    {
        _value = value;
        Error = error;
    }
    
    [Pure]
    public Tto Map<Tto>(Func<T, Tto> whenSuccess, Func<Error, Tto> whenFailure)
        => Error == null ? whenSuccess(_value) : whenFailure((Error)Error);
    
    [Pure]
    public T GetOr(T fallback) => Error == null ? _value : fallback;
    
    // Use Map<Tto> in most cases
    [Pure]
    public T GetOrThrow() => Error == null ? _value : throw new InvalidOperationException();
    
    public static implicit operator ResultOrError<T>(Error error) => ResultOrError.Failure<T>(error);
}

public static class ResultOrError
{
    public static ResultOrError<TNew> Convert<TOld, TNew>(ResultOrError<TOld> res) where TOld : TNew
    {
        return res.Map(
            value => Success<TNew>(value),
            Failure<TNew>);
    }
    
    public static ResultOrError<T> Success<T>(T value) => new(value, null);
    public static ResultOrError<T> Failure<T>(Error err) => new(default!, err);
}