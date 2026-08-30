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
    public TTo Map<TTo>(Func<T, TTo> whenSuccess, Func<Error, TTo> whenFailure)
        => Error is null ? whenSuccess(_value) : whenFailure((Error)Error);

    public ResultOrError<TTo> AndThen<TTo>(Func<T, ResultOrError<TTo>> run) 
        => Error is null ? run(_value) : ResultOrError.Failure<TTo>(Error.Value);
    
    public ResultOrError<TTo> AndThen<TTo>(Func<T, TTo> run) 
        => Error is null ? ResultOrError.Success(run(_value)) : ResultOrError.Failure<TTo>(Error.Value);

    [Pure]
    public T GetOr(T fallback) => Error is null ? _value : fallback;
    
    // Use Map<Tto> in most cases
    [Pure]
    public T GetOrThrow() => Error is null ? _value : throw new InvalidOperationException();
    
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