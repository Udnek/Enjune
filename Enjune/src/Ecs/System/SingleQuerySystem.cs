using Enjune.Ecs.EcsType;

namespace Enjune.Ecs.System;

// utility class for when you need to set up simple system
public abstract class SingleQuerySystem : ISystem
{
    protected QueryBuilder Builder = null!;

    public void OnInit(World world) => Builder = new QueryBuilder(world);

    protected abstract void BuildQuery(QueryBuilder builder);

    public abstract void OnUpdate(World world);
}