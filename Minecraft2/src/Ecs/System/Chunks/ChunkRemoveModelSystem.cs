using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System.Misc;

namespace Minecraft2.Ecs.System.Chunks;

public class ChunkRemoveModelSystem : AppSystem
{
    private World _world = null!;
    private Query<ChunkComponent, ChunkGraphicComponent> _query = null!;

    public override void OnInit(World world)
    {
        _world = world;
        _query = new QueryBuilder(world)
            .Retrieve<ChunkComponent, ChunkGraphicComponent>();
    }

    public override void OnUpdate()
    {
        _query.ForEach((entity, ref chunkComp, ref graphicLink) =>
        {
            if (!chunkComp.ToBeUnloaded) return;
            
            // dequeuing from model generation
            App.ChunkMeshGenerator.CancelJob(graphicLink.GraphicId);
            // clearing and removing api model
            App.GraphicEngine.EnqueueChunkRefit(graphicLink.GraphicId, null);
            App.GraphicEngine.EnqueueChunkRemoval(graphicLink.GraphicId);
            // recycling local model
            App.ChunkModelPool.Recycle(graphicLink.Model);
            
            _world.RemoveEntityComponent<ChunkGraphicComponent>(entity);
        });
    }
}