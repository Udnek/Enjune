using Enjune.Misc;

namespace Enjune.Data;


public record Codec<TInstance>(
    Func<TInstance, DataObject> Encode,
    Func<DataObject, TInstance> Decode)
{
    public Codec<TInstance[]> Array
    {
        get
        {
            field ??= new Codec<TInstance[]>(
                instances => { return new DataObject.Array(instances.Select(i => Encode(i)).ToArray()); }, 
                data =>
                {
                    var array = data.AsOr(DataObject.Array.Empty);
                    return array.Val.Select(v => Decode(v)).ToArray();
                });

            return field;
        }
    }
}

public delegate T Getter<in TInstance, out T>(TInstance instance);
public delegate void Setter<TInstance, in T>(ref TInstance instance, T val);

internal delegate void DecodeAndSet<TInstance>(ref TInstance instance, DataObject data); 

public static class Codecs
{
    public static readonly Codec<float> Float = new(
        v => new DataObject.Number((decimal) v),
        data => (float) data.AsOr(DataObject.Number.Zero).Decimal
    );
    
    public static readonly Codec<bool> Boolean = new(
        DataObject.Boolean.Of,
        data => data.AsOr(DataObject.Boolean.False).Val
    );
    
    public static readonly Codec<string> String = new(
        v => new DataObject.String(v),
        data => data.AsOr(DataObject.String.Empty).Val
    );
    
    public static readonly Codec<Vector3> Vector3 = NewBuilder(() => new Vector3())
            .ForField("x", v => v.X, (ref v, val) => v.X = val, Float)
            .ForField("y", v => v.Y, (ref v, val) => v.Y = val, Float)
            .ForField("z", v => v.Z, (ref v, val) => v.Z = val, Float)
            .Build();
    
    public static readonly Codec<Quaternion> Quaternion = NewBuilder(() => new Quaternion())
        .ForField("x", i => i.X, (ref i, val) => i.X = val, Float)
        .ForField("y", i => i.Y, (ref i, val) => i.Y = val, Float)
        .ForField("z", i => i.Z, (ref i, val) => i.Z = val, Float)
        .ForField("w", i => i.W, (ref i, val) => i.W = val, Float, 1)
        .Build();


    public static Codec<TInstance> ForConstructor<TInstance, T>(
        string name, Getter<TInstance, T> getter, Func<T?, TInstance> constructor, Codec<T> codec, T? defaultValue = default)
    {
        return new Codec<TInstance>(
            instance =>
            {
                var value = getter(instance);
                if (Equals(value, defaultValue)) return DataObject.Map.Empty;

                return new Dictionary<string, DataObject>(1){ {name, codec.Encode(value)} };
            }, 
            data =>
            {
                var map = data.AsOr(DataObject.Map.Empty);
                var fieldData = map.GetOrNull<DataObject>(name);
                if (fieldData == null)
                    return constructor(defaultValue);
                var value = codec.Decode(fieldData);
                return constructor(value);
            });
    }

    public static Builder<TInstance> NewBuilder<TInstance>(Func<TInstance> newInstanceCreator) 
        => new(newInstanceCreator);
    
    public sealed class Builder<TInstance>
    {
        
        private readonly Func<TInstance> _newInstanceCreator;

        private readonly List<(
            string Name, 
            Func<TInstance, DataObject?> Encoder, 
            DecodeAndSet<TInstance> DecodeAndSet)> _codecs = [];

        internal Builder(Func<TInstance> newInstanceCreator)
        {
            _newInstanceCreator = newInstanceCreator;
        }
        
        public Builder<TInstance> ForField<T>(string name, Getter<TInstance, T> getter, Setter<TInstance, T> setter, Codec<T> codec, T? defaultValue = default)
        {
            _codecs.Add((
                name,
                instance =>
                {
                    var val = getter(instance);
                    return Equals(val, defaultValue) ? null : codec.Encode(val);
                },
                (ref instance, data) => setter(ref instance, codec.Decode(data))
            ));
            return this;
        }

        private void Validate()
        {
            foreach (var group in _codecs.GroupBy(e => e.Name))
            {
                if (group.Count() > 1) 
                    Logger.Error(this, $"field name used multiple times: \"{group.Key}\"");
            }
        }
        
        public Codec<TInstance> Build()
        {
            Validate();
            _codecs.TrimExcess(); // optimizing memory
            
            return new Codec<TInstance>(
                instance =>
                {
                    var dict = new Dictionary<string, DataObject>(_codecs.Count);
                    foreach (var e in _codecs)
                    {
                        var encoded = e.Encoder(instance);
                        if (encoded is not null) dict[e.Name] = encoded;
                    }

                    return dict;
                }, 
                data =>
                {
                    var map = data.AsOr(DataObject.Map.Empty);
                    var instance = _newInstanceCreator();
                    foreach (var e in _codecs)
                    {
                        var fieldData = map.GetOrNull<DataObject>(e.Name);
                        if (fieldData == null) 
                            Logger.Warn(this, $"can not find entry for field \"{e.Name}\" in {data} while constructing {instance}");
                        else
                            e.DecodeAndSet(ref instance, fieldData);
                    }
                    return instance;
                });
        }
    }
}