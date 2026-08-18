using Enjune.Data.Codec.Misc;
using Enjune.Misc;

namespace Enjune.Data.Codec;

public sealed class InstanceMatchCodec<TInstance> : ICodec<TInstance>
{
    private readonly List<(string Name, Func<TInstance, DataObject?> EncodeIfInstance, Decoder<TInstance> Decoder)> _codecs;

    private InstanceMatchCodec(List<(string Name, Func<TInstance, DataObject?> EncodeIfInstance, Decoder<TInstance> Decoder)> codecs) 
        => _codecs = codecs;

    public DataObject Encode(TInstance instance)
    {
        foreach (var (name, encodeIfInstance, _) in _codecs)
        {
            var data = encodeIfInstance(instance);
            if (data is null) continue;
            var dict = new Dictionary<string, DataObject> { { name, data } };
            return new DataObject.Map(dict);
        }

        Logger.Error(this, $"can not encode {instance}: no suitable instance encoder");
        return DataObject.Null;
    }

    public ResultOrError<TInstance> Decode(DataObject data)
    {
        var map = data.Cast<DataObject.Map>(out var error);
        if (map is null) return 
            new Error("can not decode: " + error);
        foreach (var (name, _, decoder) in _codecs)
        {
            if (map.Val.TryGetValue(name, out var value))
                return decoder(value);
        }
        return new Error($"map {map} doesn't have any of keys: {_codecs.Select(i => i.Name).ContentToString()}");
    }
    
    public class Builder
    {
        private readonly List<(string Name, Func<TInstance, DataObject?> EncodeIfInstance, Decoder<TInstance> Decoder)> _codecs = [];
        
        public Builder IfInstance<T>(string name, ICodec<T> codec) where T : TInstance
        {
            _codecs.Add((
                name,
                i => i is T t ? codec.Encode(t) : null,
                data => DecodeResult.Convert<T, TInstance>(codec.Decode(data))));
            return this;
        }

        public InstanceMatchCodec<TInstance> Build()
        {
            CodecMisc.ValidateFieldNames(_codecs.Select(v => v.Name));
            return new InstanceMatchCodec<TInstance>(_codecs);
        }
    }
}