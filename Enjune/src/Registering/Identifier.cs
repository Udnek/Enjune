using System.Reflection;
using Enjune.Data.Codec;
using Enjune.Misc;

namespace Enjune.Registering;

public readonly record struct Identifier
{
    public static readonly ICodec<Identifier> Codec = new SimpleCodec<Identifier>(
        i => Codecs.String.Encode(i._fullName),
        data => Codecs.String.Decode(data).AndThen(Parse));
    
    private readonly string _fullName;

    public static ResultOrError<Identifier> Parse(string namespaceAndKey)
    {
        var i = namespaceAndKey.IndexOf(':');
        if (i != -1)
            return ResultOrError.Success(new Identifier(namespaceAndKey[..i], namespaceAndKey[(i + 1)..]));
        return new Error($"can not find ':' when parsing: '{namespaceAndKey}'");
    }
    
    private Identifier(string @namespace, string key) 
        => _fullName = @namespace + ":" + key;

    public static Identifier Of(Assembly assembly, string name)
    {
        var assemblyName = assembly.GetName().Name;
        if (assemblyName is null)
        {
            Logger.Error(typeof(Identifier), $"can not get assembly name for {assembly}");
            return new Identifier("null", "null");
        }
        return new Identifier(assemblyName, name);
    }

    public override string ToString() => _fullName;
}