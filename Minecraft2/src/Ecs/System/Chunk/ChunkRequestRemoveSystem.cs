using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Ecs.System;

public class ChunkRequestRemoveSystem : AppSystem
{
    private Query<ChunkComponent> _query = null!;
    private World _world = null!;

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Retrieve<ChunkComponent>();
        _world = world;
    }

    public override void OnUpdate()
    {
        _query.ForEach((entity, ref chunkComp) =>
        {
            if (chunkComp.Chunk.MarkedUnloaded)
                _world.AddEntityComponent(entity, new ToBeRemovedRequest());
        });
    }
}