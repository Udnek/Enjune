using System.Diagnostics.CodeAnalysis;
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
    
    public TTo Map<TTo>(Func<T, TTo> whenSuccess, Func<Error, TTo> whenFailure)
        => Error is null ? whenSuccess(_value) : whenFailure(Error.Value);
    
    public void Map(Action<T> whenSuccess, Action<Error> whenFailure)
    {
        if (Error is null)
            whenSuccess(_value);
        else
            whenFailure(Error.Value);
    }

    public ResultOrError<TTo> AndThen<TTo>(Func<T, ResultOrError<TTo>> run) 
        => Error is null ? run(_value) : ResultOrError.Failure<TTo>(Error.Value);
    
    public ResultOrError<TTo> AndThen<TTo>(Func<T, TTo> run) 
        => Error is null ? ResultOrError.Success(run(_value)) : ResultOrError.Failure<TTo>(Error.Value);
    
    
    [Pure]
    [Obsolete("use GetIsFailed")]
    public T GetOrThrow() => Error is null ? _value : throw new Exception(Error);

    /// <summary>
    /// Returns true if err. Basic usage example:
    /// if (resultOrError.GetIsFailed(out var result, out var error))
    ///     return "can not init: " + error
    /// </summary>
    /// <param name="value"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public bool GetIsFailed([MaybeNullWhen(true)] out T value, out Error? error)
    {
        value = _value;
        error = Error;
        return Error != null;
    }
    
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