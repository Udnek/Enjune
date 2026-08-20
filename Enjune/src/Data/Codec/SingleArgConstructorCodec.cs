using Enjune.Data.Codec.Misc;
using Enjune.Misc;

namespace Enjune.Data.Codec;

public sealed class SingleArgConstructorCodec<TInstance, TArg>(
    Func<TArg, TInstance> constructor,
    Getter<TInstance, TArg> getter,
    ICodec<TArg> codec)
    : ICodec<TInstance>
{
    public ResultOrError<DataObject> Encode(TInstance instance) => codec.Encode(getter(instance));

    public ResultOrError<TInstance> Decode(DataObject instance)
    {
        return codec.Decode(instance).Map(
            val => ResultOrError.Success(constructor(val)),
            err => new Error("can not construct: " + err));
    }
}