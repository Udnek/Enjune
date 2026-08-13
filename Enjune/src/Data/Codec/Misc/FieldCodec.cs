namespace Enjune.Data.Codec.Misc;

public interface IFieldCodec<TInstance>
{
    DataObject GetAndEncode(TInstance instance);
    Error? DecodeAndSet(ref TInstance instance, DataObject data);
}

public sealed class FieldCodec<TInstance, TField>(
    Getter<TInstance, TField> getter, 
    Setter<TInstance, TField> setter, 
    ICodec<TField> codec) : IFieldCodec<TInstance>
{
    public DataObject GetAndEncode(TInstance instance) => codec.Encode(getter(instance));

    public Error? DecodeAndSet(ref TInstance instance, DataObject data)
    {
        var result = codec.Decode(data);
        // not using map cause ref can not be used in lambdas
        if (result.Error != null) 
            return "can not decode: " + result.Error;
        setter(ref instance, result.GetOrThrow());
        return null;
    }
}