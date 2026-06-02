using Enjune.Misc;

namespace Enjune.Data;

public static partial class Codecs
{

    private static void ValidateFieldNames(IEnumerable<string> names)
    {
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var group in names.GroupBy(e => e))
        {
            if (group.Count() > 1) 
                // ReSharper disable once PossibleMultipleEnumeration
                Logger.Error(typeof(Codecs), $"field name used multiple times: \"{group.Key}\" in {names.ContentToString()}");
        }
    }


    public class EitherBuilder<TInstance>
    {
        private readonly List<(string Name, Codec<TInstance>)> _codecs = [];

        public EitherBuilder<TInstance> OrIfNotNull<T>(string optionName, Func<TInstance, T?> selector, Codec<T> codec, T? defaultValue = default)
        {
            _codecs.Add((
                optionName,
                instance =>
                {
                    var value = selector(instance);
                    if (value is null) return null;
                    return Equals(value, defaultValue) ? null : codec.Encode(value);
                }, 
                data =>
                {
                    if (data is null)  // TODO REMOVE OR MAKE DEBUG ONLY
                        Logger.Warn(this, $"can not find entry for field \"{optionName}\" in {data}, using default: {defaultValue}");
                    var value = data is null ? defaultValue : codec.Decode(data);
                    return value;
                }));
            return this;
        }
        
        public Codec<TInstance> Build()
        {
            ValidateFieldNames(_codecs.Select(e => e.Name));
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
                    var args = new object?[_codecs.Count];
                    for (var i = 0; i < _codecs.Count; i++)
                    {
                        var e = _codecs[i];
                        var fieldData = map.GetOrNull<DataObject>(e.Name);
                        args[i] = e.Decoder(fieldData);
                    }

                    return constructor(args);
                });
        }
    }
    
    public class BigConstructorBuilder<TInstance>(Func<object?[], TInstance> constructor)
    {
        private readonly List<(
            string Name, 
            Func<TInstance, DataObject?> Encoder, 
            Func<DataObject?, object?> Decoder)> _codecs = [];

        public BigConstructorBuilder<TInstance> ForField<T>(string name, Getter<TInstance, T> getter, Codec<T> codec, T? defaultValue = default)
        {
            _codecs.Add((
                name,
                instance =>
                {
                    var val = getter(instance);
                    return Equals(val, defaultValue) ? null : codec.Encode(val);
                }, 
                data =>
                {
                    if (data is null)  // TODO REMOVE OR MAKE DEBUG ONLY
                        Logger.Warn(this, $"can not find entry for field \"{name}\" in {data}, using default: {defaultValue}");
                    var value = data is null ? defaultValue : codec.Decode(data);
                    return value;
                }));
            return this;
        }
        
        public Codec<TInstance> Build()
        {
            ValidateFieldNames(_codecs.Select(e => e.Name));
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
                    var args = new object?[_codecs.Count];
                    for (var i = 0; i < _codecs.Count; i++)
                    {
                        var e = _codecs[i];
                        var fieldData = map.GetOrNull<DataObject>(e.Name);
                        args[i] = e.Decoder(fieldData);
                    }

                    return constructor(args);
                });
        }
    }
    
    public class EmptyConstructorBuilder<TInstance>(Func<TInstance> emptyConstructor)
    {
        private readonly List<(
            string Name, 
            Func<TInstance, DataObject?> Encoder, 
            DecodeAndSet<TInstance> DecodeAndSet)> _codecs = [];

        public EmptyConstructorBuilder<TInstance> ForField<T>(string name, Getter<TInstance, T> getter, Setter<TInstance, T> setter, Codec<T> codec, T? defaultValue = default)
        {
            _codecs.Add((
                name,
                instance =>
                {
                    var value = getter(instance);
                    return Equals(value, defaultValue) ? null : codec.Encode(value);
                },
                (ref instance, data) =>
                {
                    if (data is null)  // TODO REMOVE OR MAKE DEBUG ONLY
                        Logger.Warn(this, $"can not find entry for field \"{name}\" in {data}, using default: {defaultValue}");
                    var value = data is null ? defaultValue : codec.Decode(data);
                    setter(ref instance, value!);
                }));
            return this;
        }
        
        public Codec<TInstance> Build()
        {
            ValidateFieldNames(_codecs.Select(e => e.Name));
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
                    var instance = emptyConstructor();
                    foreach (var e in _codecs)
                    {
                        var fieldData = map.GetOrNull<DataObject>(e.Name);
                        e.DecodeAndSet(ref instance, fieldData);
                    }
                    return instance;
                });
        }
    }
}