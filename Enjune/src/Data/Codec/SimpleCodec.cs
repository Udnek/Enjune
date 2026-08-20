using Enjune.Data.Codec.Misc;
using Enjune.Misc;

namespace Enjune.Data.Codec;

public sealed class SimpleCodec<TInstance>(
    Encoder<TInstance> encoder,
    Decoder<TInstance> decoder)
    : ICodec<TInstance>
{
    public ResultOrError<DataObject> Encode(TInstance instance) => encoder(instance);
    public ResultOrError<TInstance> Decode(DataObject data) => decoder(data);
}