using Enjune.Data.Codec.Misc;
using Enjune.Misc;

namespace Enjune.Data.Codec;

public interface IMapCodec<TInstance> : ICodec<TInstance>
{
    void ForEach(Consumer<(string Name, IFieldCodec<TInstance> FieldCodec)> consumer);
}

public sealed class MapCodec<TInstance> : IMapCodec<TInstance>
{
    private readonly Dictionary<string, IFieldCodec<TInstance>> _fieldCodecs;
    private readonly EmptyConstructor<TInstance> _constructor;

    private MapCodec(EmptyConstructor<TInstance> constructor, Dictionary<string, IFieldCodec<TInstance>> fieldCodecs)
    {
        _constructor = constructor;
        _fieldCodecs = fieldCodecs;
    }

    public void ForEach(Consumer<(string Name, IFieldCodec<TInstance> FieldCodec)> consumer)
    {
        foreach (var (name, codec) in _fieldCodecs) 
            consumer((name, codec));
    }
    
    public ResultOrError<DataObject> Encode(TInstance instance)
    {
        Dictionary<string, DataObject> fields = [];
        foreach (var (name, codec) in _fieldCodecs)
        {
            var result = codec.GetAndEncode(instance);
            if (result.Error != null)
                return new Error($"can not encode field {name}: {result.Error}");
            fields[name] = result.GetOrThrow();
        }

        return ResultOrError.Success<DataObject>(fields);
    }

    public ResultOrError<TInstance> Decode(DataObject data)
    {
        var mapData = data.Cast<DataObject.Map>(out var error);
        if (mapData is null)
            return new Error("data is not map: " + error);
        
        var instance = _constructor();
        foreach (var (name, codec) in _fieldCodecs)
        {
            if (mapData.Val.TryGetValue(name, out var fieldData))
            {
                var fieldErr = codec.DecodeAndSet(ref instance, fieldData);
                if (fieldErr != null)
                    return new Error($"can not decode field {name}: " + fieldErr);
            }
            else 
                return new Error($"map {mapData} doesn't have key {name}");
        }
        return ResultOrError.Success(instance);
    }

    public class Builder(EmptyConstructor<TInstance> constructor)
    {
        private readonly Dictionary<string, IFieldCodec<TInstance>> _fieldCodecs = [];
        
        public Builder ForField(string name, IFieldCodec<TInstance> fieldCodec)
        {
            _fieldCodecs.Add(name, fieldCodec);
            return this;
        }

        public Builder ForField<TField>(string name,
            Getter<TInstance, TField> getter,
            Setter<TInstance, TField> setter,
            ICodec<TField> codec)
        {
            return ForField(name, new FieldCodec<TInstance,TField>(getter, setter, codec));
        }
        
        public MapCodec<TInstance> Build()
        {
            CodecMisc.ValidateFieldNames(_fieldCodecs.Keys);
            return new MapCodec<TInstance>(constructor, _fieldCodecs);
        }
    }
}