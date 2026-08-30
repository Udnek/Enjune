using Enjune.Ecs.EcsType;

namespace Enjune.Ecs.System;

// utility class for when you need to set up simple system
public abstract class SingleQuerySystem : ISystem
{
    protected Query Query = null!;
    
    public void InitializeQueries(World world) => Query = BuildQuery(new Query.Builder(world));
    
    protected abstract Query BuildQuery(Query.Builder builder);

    public abstract void Update(World world);
}