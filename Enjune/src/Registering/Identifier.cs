using System.Reflection;
using Enjune.Data;
using Enjune.Data.Codec;
using Enjune.Misc;

namespace Enjune.Registering;

public readonly struct Identifier : IEquatable<Identifier>
{
    public static readonly SingleArgConstructorCodec<Identifier, string> Codec = Codecs
        .ForOneArgConstructor(Parse, i => i._fullName, Codecs.String);

    public static readonly Identifier NullFallback = new("null", "null");
    
    private readonly string _fullName;
    private readonly int _hash;

    public static Identifier Parse(string namespaceAndKey)
    {
        var i = namespaceAndKey.IndexOf(':');
        if (i != -1) 
            return new Identifier(namespaceAndKey[..i], namespaceAndKey[(i + 1)..]);
        Logger.Error(typeof(Identifier),$"can not find ':' when parsing: '{namespaceAndKey}'");
        return NullFallback;
    }
    
    private Identifier(string @namespace, string key)
    {
        _fullName = @namespace + ":" + key;
        _hash = _fullName.GetHashCode();
    }

    public static Identifier Of(Assembly assembly, string name)
    {
        var assemblyName = assembly.GetName().Name;
        if (assemblyName is null)
        {
            Logger.Error(typeof(Identifier), $"can not get assembly name for {assembly}");
            return NullFallback;
        }
        return new Identifier(assemblyName, name);
    }

    public override string ToString() => _fullName;
    public override int GetHashCode() => _hash;
    public override bool Equals(object? obj) => _hash == obj?.GetHashCode();
    public bool Equals(Identifier other) => _hash == other._hash;
    public static bool operator ==(Identifier left, Identifier right) => left.Equals(right);
    public static bool operator !=(Identifier left, Identifier right) => !(left == right);
}