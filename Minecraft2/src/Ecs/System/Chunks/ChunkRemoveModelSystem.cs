using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System.Misc;
using Minecraft2.Terrain;

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
        _query.ForEach((entity, ref chunkComp, ref chunkGrx) =>
        {
            if (!chunkComp.ToBeUnloaded) return;
            
            // dequeuing from model generation
            App.ChunkMeshGenerator.EnqueueJobCancel(chunkGrx.GraphicId);
            // clearing and removing api model
            App.GraphicEngine.EnqueueChunkRefit(chunkGrx.GraphicId, null);
            App.GraphicEngine.EnqueueChunkRemoval(chunkGrx.GraphicId);
            
            _world.RemoveEntityComponent<ChunkGraphicComponent>(entity);
        });
        
        // recycling models
        while (App.ChunkMeshGenerator.TryDequeueCancelled(out Guid id, out var job))
        {
            //Logger.Highlight(this, $"got canceled mesh for {job.TargetChunk.Position}");
            App.ChunkModelPool.Recycle(job.Model);
        }
    }
}