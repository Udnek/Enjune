using System.Diagnostics.CodeAnalysis;
using Enjune.Misc;

namespace Enjune.Registering;


public interface IRegistry<T>
{ 
    ResourceKey<T> Register(Identifier id, T value);
    T GetOrThrow(Identifier id);
    T? GetOrNull(Identifier id);
    bool TryGet(Identifier id, [MaybeNullWhen(false)] out T value);
}

public sealed class Registry<T> : IRegistry<T>
{
    private readonly Dictionary<Identifier, T> _idToValue = [];
    
    public ResourceKey<T> Register(Identifier id, T value)
    {
        if (_idToValue.ContainsKey(id))
            Logger.Error(this, $"can not register value because id already presented: {id}");
        else
            _idToValue[id] = value;
        return new ResourceKey<T>(this, id);
    }

    public T? GetOrNull(Identifier id)
    {
        _idToValue.TryGetValue(id, out var value);
        return value;
    }

    public T GetOrThrow(Identifier id) => _idToValue[id];

    public bool TryGet(Identifier id, [MaybeNullWhen(false)] out T value) 
        => _idToValue.TryGetValue(id, out value);

    public override string ToString() => Logger.GetTypeName<Registry<T>>();
}