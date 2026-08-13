using Enjune.Data.Codec.Misc;

namespace Enjune.Data.Codec;

public sealed class SingleArgConstructorCodec<TInstance, TArg>(
    Func<TArg, TInstance> constructor,
    Getter<TInstance, TArg> getter,
    ICodec<TArg> codec)
    : ICodec<TInstance>
{
    public DataObject Encode(TInstance instance) => codec.Encode(getter(instance));

    public DecodeResult<TInstance> Decode(DataObject data)
    {
        return codec.Decode(data).Map(
            val => DecodeResult.Success(constructor(val)),
            err => new Error("can not decode: " + err));
    }
}