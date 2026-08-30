using Enjune.Ecs.EcsType;

namespace Enjune.Ecs.System;

// utility class for when you need to set up simple system
[Obsolete]
public abstract class SingleLegacyQuerySystem : ISystem
{
    protected LegacyQuery Query = null!;
    
    public void InitializeQueries(World world) => Query = BuildQuery(new LegacyQuery.Builder(world));
    
    protected abstract LegacyQuery BuildQuery(LegacyQuery.Builder builder);

    public abstract void Update(World world);
}