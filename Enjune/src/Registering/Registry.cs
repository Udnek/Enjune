using System.Diagnostics.CodeAnalysis;
using Enjune.Misc;

namespace Enjune.Registering;


public interface IRegistry<T>
{
    public T Register(Identifier id, T value, bool useAsDefault = false);
    public T GetOrThrow(Identifier id);
    public T? GetOrDefault(Identifier id);
    public bool TryGet(Identifier id, [MaybeNullWhen(false)] out T value);
}

public sealed class Registry<T> : IRegistry<T>
{
    private readonly Dictionary<Identifier, T> _idToValue = [];
    private T? _fallback;
    
    public T Register(Identifier id, T value, bool useAsDefault = false)
    {
        if (_idToValue.ContainsKey(id))
        {
            Logger.Error(this, $"can not register value 'cause id already presented: {id}");
            return value;
        }
        _idToValue[id] = value;
        if (useAsDefault) _fallback = value;
        return value;
    }

    public T GetOrThrow(Identifier id) => _idToValue[id];

    public T? GetOrDefault(Identifier id) => _idToValue.TryGetValue(id, out var value) ? value : _fallback;

    public bool TryGet(Identifier id, [MaybeNullWhen(false)] out T value) 
        => _idToValue.TryGetValue(id, out value);
}