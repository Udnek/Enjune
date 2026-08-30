using Enjune.Ecs.Component;

namespace Enjune.Ecs.EcsType;


public delegate void ForEachRef<T1, T2>(ref T1 t1, ref T2 t2) where T1 : struct, IComponent where T2 : struct, IComponent;

public sealed class Query<T1, T2>(World world, Signature include, Signature exclude) where T1 : struct, IComponent where T2 : struct, IComponent
{
    private readonly World _world = world;
    private readonly Signature _include = include;
    private readonly Signature _exclude = exclude;
    
    private readonly List<Archetype> _cachedArchetypes = [];
    private readonly List<(Column<T1> Column1, Column<T2> Column2)> _cachedColumns = [];
    private int _cacheVersion = 0;

    private void ValidateCache()
    {
        if (_world.CacheVersion != _cacheVersion)
        {
            _cacheVersion = _world.CacheVersion;
            _cachedArchetypes.Clear();
            foreach (var archetype in _world.QueryArchetypes(_include, _exclude))
                _cachedArchetypes.Add(archetype);
        }
    }

    public void ForEach(ForEachRef<T1, T2> action)
    {
        ValidateCache();
        foreach (var (col1, col2) in _cachedColumns)
        {
            for (int i = 0; i < col1.Count; i++)
            {
                action(ref col1[i], ref col2[i]);
            }
        }
    }


}

[Obsolete]
public sealed class Query(World world, Signature include, Signature exclude)
{
    private readonly World _world = world;
    private readonly Signature _include = include;
    private readonly Signature _exclude = exclude;

    private readonly List<Archetype> _cachedArchetypes = [];
    private int _cacheVersion = 0;
        
    // TODO: Ensure cache invalidation is in place in World logic
    private void ValidateCache()
    {
        if (_world.CacheVersion == _cacheVersion) 
            return;
        
        _cacheVersion = _world.CacheVersion;
        _cachedArchetypes.Clear();
        foreach (var archetype in _world.QueryArchetypes(_include, _exclude)) 
            _cachedArchetypes.Add(archetype);
    }

    public void ForEachArchetype(Action<Archetype> action)
    {
        ValidateCache();
        foreach (var archetype in _cachedArchetypes)
            action(archetype);
    }

    public static Builder For(World world) => new(world);

    public sealed class Builder(World world)
    {
        private readonly World _world = world;
        private readonly Signature.Builder _includeBuilder = new(world);
        private readonly Signature.Builder _excludeBuilder = new(world);

        public Builder With<T>() where T : IComponent
        {
            _includeBuilder.RegisterComponent<T>();
            return this;
        }
        
        public Builder Without<T>() where T : IComponent
        {
            _excludeBuilder.RegisterComponent<T>();
            return this;
        }
        
        public Query Build() => new(_world, _includeBuilder.Build(), _excludeBuilder.Build());
    }
}