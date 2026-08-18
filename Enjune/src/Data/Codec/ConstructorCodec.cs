using System.Text;
using Enjune.Data.Codec.Misc;
using Enjune.Misc;

namespace Enjune.Data.Codec;

public sealed class ConstructorCodec<TInstance> : ICodec<TInstance>
{
    private delegate object? Decoder(DataObject data, out Error? error);
    
    private readonly List<(
        string Name, 
        Func<TInstance, DataObject> Encoder, 
        Decoder Decoder)> _codecs;
    
    private readonly Func<List<object?>, TInstance> _constructor;

    private ConstructorCodec(Func<List<object?>, TInstance> constructor, List<(string, Func<TInstance, DataObject>, Decoder)> codecs)
    {
        _constructor = constructor;
        _codecs = codecs;
    }
    
    public DataObject Encode(TInstance instance)
    {
        var dict = _codecs.Select(tuple =>
        {
            var (name, encoder, _) = tuple;
            return (name, encoder(instance));
        }).ToDictionary();
        return new DataObject.Map(dict);
    }

    public ResultOrError<TInstance> Decode(DataObject data)
    {
        var mapData = data.Cast<DataObject.Map>(out var error);
        if (mapData is null)
            return DecodeResult.Failure<TInstance>("can not decode: " + error);
        
        var args = new List<object?>(_codecs.Count);
        foreach (var (name, _, decoder) in _codecs)
        {
            if (mapData.Val.TryGetValue(name, out var value))
            {
                var arg = decoder(value, out var fieldErr);
                if (fieldErr != null)
                    return new Error($"can not decode field {name}: " + fieldErr);
                args.Add(arg);
            }
            return new Error($"map {mapData} doesn't have key {name}");
        }
        
        return DecodeResult.Success(_constructor(args));
    }
    
    public class Builder(Func<List<object?>, TInstance> constructor)
    {
        private readonly List<(
            string Name, 
            Func<TInstance, DataObject> Encoder, 
            Decoder Decoder)> _codecs = [];
        
        public Builder ForField<TField>(string name, Getter<TInstance, TField> getter, ICodec<TField> fieldCodec)
        {
            _codecs.Add((
                name,
                instance => fieldCodec.Encode(getter(instance)),
                (data, out error) =>
                {
                    var result = fieldCodec.Decode(data);
                    if (result.Error != null)
                        error = $"can not decode field {name}: " + result.Error;
                    else
                        error = null;
                    return result;
                }));
            return this;
        }
        
        public ConstructorCodec<TInstance> Build()
        {
            CodecMisc.ValidateFieldNames(_codecs.Select(v => v.Name));
            return new ConstructorCodec<TInstance>(constructor, _codecs);
        }
    }
}