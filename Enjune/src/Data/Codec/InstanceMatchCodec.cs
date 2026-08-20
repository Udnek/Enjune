using Enjune.Data.Codec.Misc;
using Enjune.Misc;

namespace Enjune.Data.Codec;

public sealed class InstanceMatchCodec<TInstance> : ICodec<TInstance>
{
    private readonly List<(string Name, Func<TInstance, ResultOrError<DataObject>?> EncodeIfInstance, Decoder<TInstance> Decoder)> _codecs;

    private InstanceMatchCodec(List<(string Name, Func<TInstance, ResultOrError<DataObject>?> EncodeIfInstance, Decoder<TInstance> Decoder)> codecs) 
        => _codecs = codecs;

    public ResultOrError<DataObject> Encode(TInstance instance)
    {
        foreach (var (name, encodeIfInstance, _) in _codecs)
        {
            var result = encodeIfInstance(instance);
            if (result is null) continue;
            if (result.Value.Error != null)
                return new Error($"can not encode for type {name}");
            
            var data = result.Value.GetOrThrow();
            var dict = new Dictionary<string, DataObject>
            {
                [name] = data
            };
            return ResultOrError.Success<DataObject>(dict);
        }

        return new Error($"can not encode {instance}: no suitable instance encoder");
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
        private readonly List<(string Name, Func<TInstance, ResultOrError<DataObject>?> EncodeIfInstance, Decoder<TInstance> Decoder)> _codecs = [];
        
        public Builder IfInstance<T>(string name, ICodec<T> codec) where T : TInstance
        {
            _codecs.Add((
                name,
                i => i is T t ? codec.Encode(t) : null,
                data => ResultOrError.Convert<T, TInstance>(codec.Decode(data))));
            return this;
        }

        public InstanceMatchCodec<TInstance> Build()
        {
            CodecMisc.ValidateFieldNames(_codecs.Select(v => v.Name));
            return new InstanceMatchCodec<TInstance>(_codecs);
        }
    }
}