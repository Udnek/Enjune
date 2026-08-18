using Enjune.Data.Codec;
using Enjune.Misc;

namespace Enjune.Registering;

public sealed record RegistryReference<T> where T : notnull
{
    public readonly Identifier RegistryId;
    public readonly Identifier ItemId;
    private T? _value = default;
    private bool _triedGetting = false;
    
    public RegistryReference(Identifier registryId, Identifier itemId)
    {
        RegistryId = registryId;
        ItemId = itemId;
    }

    public static ICodec<RegistryReference<T>> Codec =>
        field ??= Codecs
            .ForConstructor(args => new RegistryReference<T>((Identifier)args[0]!, (Identifier)args[1]!))
            .ForField("registry_id", i => i.RegistryId, Identifier.Codec)
            .ForField("item_id", i => i.ItemId, Identifier.Codec)
            .Build();

    public T? Get(out Error? error)
    {
        if (_triedGetting)
        {
            if (_value is null)
            {
                error = "could not get item previously; cached as null";
                return default;
            }
            else
            {
                error = null;
                return _value;      
            }
        }

        _value = default;
        _triedGetting = true;
        
        var registry = Registries.All.Get(RegistryId, out var regErr);
        if (registry is null)
        {
            error = "missing registry: " + regErr;
            return default;
        }

        var item = registry.Get(ItemId, out var itemErr);
        if (item is null)
        {
            error = "missing item key: " + itemErr;
            return default;
        }
        if (item is not T typedItem)
        {
            error = $"item is of incorrect type; got: {Logger.GetTypeName(item.GetType())}, expected: {Logger.GetTypeName<T>()}";
            return default;
        }

        Logger.Info(this, $"Successfully got item {ItemId} in registry {RegistryId}; caching");
        _value = typedItem;
        error = null;
        return typedItem;
    }
}