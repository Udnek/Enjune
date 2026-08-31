using System.Diagnostics.Contracts;
using Enjune.Misc;

namespace Enjune.Registering;


public interface IRegistry<out T>
{
    [Pure]
    T? Get(Identifier id, out Error? error);
}

public sealed class WritableRegistry<T> : IRegistry<T> where T: notnull
{
    private readonly Identifier _id;
    private readonly Dictionary<Identifier, T> _idToValue = new(0);
    
    public static WritableRegistry<T> CreateAndRegister(Identifier registryId)
    {
        var registry = new WritableRegistry<T>(registryId);
        Registries.All.RegisterUnsafe(registryId, registry, out var error);
        if (error is not null)
            Logger.Error(typeof(WritableRegistry<T>), $"Can not register {registry}: {error}");
        return registry;
    }

    internal static WritableRegistry<T> CreateRootRegistry(Identifier id) => new(id);

    private WritableRegistry(Identifier id) => _id = id;

    public RegistryReference<T> Register(Identifier id, T value)
    {
        if (_idToValue.ContainsKey(id))
            Logger.Warn(this, $"Item with id already presented: {id}; replacing");
        _idToValue[id] = value;
        return CreateReference(id);
    }

    public void Register(RegistryReference<T> reference, T value) => Register(reference.ItemId, value);

    [Pure]
    public RegistryReference<T> CreateReference(Identifier itemId) => new(_id, itemId);

    [Pure]
    public Identifier? GetId(T target)
    {
        foreach (var (key, value) in _idToValue)
        {
            if (Equals(value, target))
                return key;
        }

        return null;
    }
    
    [Pure]
    public T? Get(Identifier id, out Error? error)
    {
        if (_idToValue.TryGetValue(id, out var value))
        {
            error = null;
            return value;
        }

        error = $"registry doesn't contain {id}";
        return default;
    }
    
    public override string ToString() => $"{Logger.GetTypeName<WritableRegistry<T>>()}[{_id}]";
    
    // parent interface
    
    public void RegisterUnsafe(Identifier id, object value, out Error? error)
    {
        if (value is not T typed)
        {
            error = $"{this} expects {Logger.GetTypeName<T>()}, but got {value}";
            return;
        }

        error = null;
        Register(id, typed);
    }
}

