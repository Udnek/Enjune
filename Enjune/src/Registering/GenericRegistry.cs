namespace Enjune.Registering;

public class GenericRegistry<T>
{
    private static readonly Dictionary<Type, (string Name, ICodeс<object> Codec)> TypeToCodec = new();
    private static readonly Dictionary<Identifier, ICodeс<object>> NameToCodec = new();
    
    public static void Register<T>(string name, ICodec<T> codec) where T : notnull, IComponent
    {
        var objCodec = new SimpleCodec<object>(
            i => codec.Encode((T)i),
            data => DecodeResult.Convert<T, object>(codec.Decode(data)));

        TypeToCodec[typeof(T)] = (name, objCodec);
        NameToCodec[name] = objCodec;
    }
}