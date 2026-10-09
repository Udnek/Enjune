using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Ecs.System.Chunk;

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
            chunkWorld.Unload(chunkComp.Pos);
            _world.AddEntityComponent(entity, new ToBeRemovedRequest());
        });
    }
}