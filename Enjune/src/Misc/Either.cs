namespace Enjune.Misc;

public abstract record Either<TLeft, TRight>
{
    private Either() {}

    public abstract void Map(Consumer<TLeft> ifLeft, Consumer<TRight> ifRight);
    
    public static implicit operator Either<TLeft, TRight>(TLeft value) => Either.Left<TLeft, TRight>(value);
    public static implicit operator Either<TLeft, TRight>(TRight value) => Either.Right<TLeft, TRight>(value);
    
    public sealed record Left(TLeft Value) : Either<TLeft, TRight>
    {
        public override void Map(Consumer<TLeft> ifLeft, Consumer<TRight> ifRight) => ifLeft(Value);
    }

    public sealed record Right(TRight Value) : Either<TLeft, TRight>
    {
        public override void Map(Consumer<TLeft> ifLeft, Consumer<TRight> ifRight) => ifRight(Value);
    }
}

public static class Either
{
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft value) => new Either<TLeft, TRight>.Left(value);
    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight value) => new Either<TLeft, TRight>.Right(value);
}