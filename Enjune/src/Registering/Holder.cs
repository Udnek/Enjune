using Enjune.Data.Codec;
using Enjune.Misc;

namespace Enjune.Registering;

[Obsolete]
public interface IHolder<T>
{
    public T GerOr(T fallback);
    public T? GetOrNull();
    
    public class Direct(T value) : IHolder<T>
    {
        public T GerOr(T fallback) => value;
        public T GetOrNull() => value;
    }

    public class RegistryReference(Registry<T> registry, Identifier id) : IHolder<T>
    {
        public T GerOr(T fallback)
        {
            if (registry.TryGet(id, out var value))
                return value;
            Logger.Warn(this, $"registry {Logger.GetAuthorName(registry)} doesn't have id {id}");
            return fallback;
        }

        public T? GetOrNull() => registry.GetOrNull(id);
    }
}