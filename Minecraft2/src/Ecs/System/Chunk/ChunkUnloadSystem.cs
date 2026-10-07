using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Ecs.System;

public class ChunkUnloadSystem : AppSystem
{
    private Query<ChunkComponent> _query = null!;
    private World _world = null!;

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Excluding<ToBeRemovedRequest>()
            .Retrieve<ChunkComponent>();
        _world = world;
    }
    
    public override void OnUpdate()
    {
        var chunkWorld = App.ChunkWorld;
        _query.ForEach((entity, ref chunkComp) =>
        {
            if (!chunkComp.ToBeUnloaded) return;
            Logger.Highlight(this, $"{entity}: {chunkComp}");
            Logger.Highlight(this, $"{entity}: {_world.GetEntityComponents(entity).ContentToString()}");
            chunkWorld.Unload(chunkComp.Pos);
            _world.AddEntityComponent(entity, new ToBeRemovedRequest());
        });
    }
}