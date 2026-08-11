using Enjune.Data.Codec;
using Enjune.Misc;

namespace Enjune.Registering;

public sealed class ResourceKey<T>(Registry<T> registry, Identifier id)
{
    private readonly Registry<T> _registry = registry;
    private readonly Identifier _id = id;

    public static ICodec<ResourceKey<T>> CreateCodec(ICodec<Registry<T>> registryCodec) => Codecs
        .ForConstructor(args => new ResourceKey<T>((Registry<T>)args[0]!, (Identifier)args[1]!))
        .ForField("registry", i => i._registry, registryCodec)
        .ForField("id", i => i._id, Identifier.Codec)
        .Build();
    
    public T GerOr(T fallback)
    {
        if (_registry.TryGet(_id, out var value))
            return value;
        Logger.Warn(this, $"registry {Logger.GetTypeName(_registry.GetType())} doesn't have id {_id}");
        return fallback;
    }
    public T? GetOrNull() => _registry.GetOrNull(_id);
    public T GetOrThrow() => _registry.GetOrThrow(_id);

    public override string ToString() => $"{Logger.GetTypeName<ResourceKey<T>>()}[{_id}]";
}