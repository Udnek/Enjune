using Enjune.Data.Codec.Misc;
using Enjune.Misc;

namespace Enjune.Data.Codec;

public sealed class SimpleCodec<TInstance>(
    Func<TInstance, DataObject> encoder,
    Decoder<TInstance> decoder)
    : ICodec<TInstance>
{
    public DataObject Encode(TInstance instance) => encoder(instance);
    public ResultOrError<TInstance> Decode(DataObject data) => decoder(data);
}