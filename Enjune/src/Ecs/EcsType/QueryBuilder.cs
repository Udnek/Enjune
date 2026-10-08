using Enjune.Ecs.Component;

namespace Enjune.Ecs.EcsType;

public delegate void ForEachDelegate(Entity entity);

public class Query(World world, Signature include, Signature exclude)
{
    private readonly World _world = world;
    private readonly Signature _include = include;
    private readonly Signature _exclude = exclude;
    private static List<(Entity[], int count)> _cache = [];
    private static int _cacheVersion = -1;
    internal List<(Entity[], int count)> GetCache(World world, Signature include, Signature exclude)
    {
        if (world.CacheVersion == _cacheVersion) return _cache;
        _cache.Clear();
        foreach (var archetype in world.QueryArchetypes(include, exclude))
        {
            _cache.Add((archetype.GetEntities(), archetype.Count));
        }
        _cacheVersion = world.CacheVersion;
        return _cache;
    }
    public void ForEach(ForEachDelegate action)
    {
        _world.Lock();
        var cache = GetCache(_world, _include, _exclude);
        foreach (var (entities, count) in cache)
        {
            for (int i = 0; i < count; i++)
            {
                action(entities[i]);
            }
        }
        _world.Unlock();
    }
}

public sealed partial class QueryBuilder(World world)
{
    private readonly World _world = world;
    private readonly Signature.Builder _includeBuilder = new(world);
    private readonly Signature.Builder _excludeBuilder = new(world);

    public QueryBuilder Including<T>() where T : struct, IComponent
    {
        _includeBuilder.RegisterComponent<T>();
        return this;
    }

    public QueryBuilder Excluding<T>() where T : struct, IComponent
    {
        _excludeBuilder.RegisterComponent<T>();
        return this;
    }
    public Query Retrieve()
    {
        return new Query(_world, _includeBuilder.Build(), _excludeBuilder.Build());
    }
}