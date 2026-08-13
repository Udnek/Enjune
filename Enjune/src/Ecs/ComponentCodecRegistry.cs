using Enjune.Data;
using Enjune.Data.Codec;
using Enjune.Data.Codec.Misc;
using Enjune.Ecs.Component;
using Enjune.Misc;

namespace Enjune.Ecs;

// can be used when working with unbound values
public static class ComponentCodecRegistry
{
    private static readonly Dictionary<Type, (string Name, ICodec<object> Codec)> TypeToCodec = new();
    private static readonly Dictionary<string, ICodec<object>> NameToCodec = new();

    public static readonly ICodec<object> Codec = new SimpleCodec<object>(
        i =>
        {
            if (TypeToCodec.TryGetValue(i.GetType(), out var codec))
                return codec.Codec.Encode(i);

            Logger.Error(typeof(ComponentCodecRegistry), $"Can not encode {i} cause codec wasn't found");
            return DataObject.Null;
        },
        data =>
        {
            var map = data.Cast<DataObject.Map>(out var error);
            if (map is null)
                return new Error($"can not decode: {error}");
            var keys = map.Val.Keys.ToArray();
            if (keys.Length != 1)
                return new Error($"expected only one key in map: {map}");
            if (NameToCodec.TryGetValue(keys.First(), out var codec))
                return codec.Decode(map.Val[keys.First()]);
            return new Error($"can not find codec for name {keys.First()}");
        }
    );
    
    public static void Register<T>(string name, ICodec<T> codec) where T : notnull, IComponent
    {
        var objCodec = new SimpleCodec<object>(
            i => codec.Encode((T)i),
            data => DecodeResult.Convert<T, object>(codec.Decode(data)));

        TypeToCodec[typeof(T)] = (name, objCodec);
        NameToCodec[name] = objCodec;
    }
}