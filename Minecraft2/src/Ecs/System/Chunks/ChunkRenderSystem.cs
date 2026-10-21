using System.Diagnostics;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System.Misc;
using Minecraft2.Misc;
using OpenTK.Mathematics;

namespace Minecraft2.Ecs.System.Chunks;

public class ChunkRenderSystem : AppSystem
{
    private Query<ChunkComponent, GraphicLinkComponent> _query = null!;

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Retrieve<ChunkComponent, GraphicLinkComponent>();
    }

    public override void OnUpdate()
    {
        _query.ForEach((_, ref chunkComp, ref graphicLink) =>
        {
            var chunk = chunkComp.Chunk;
            if (!chunk.IsDirty) return;
            
            chunk.IsDirty = false;
            // mesh generation
            App.ChunkMeshGenerator.Enqueue((graphicLink.GraphicId, chunk, graphicLink.Model));
        });
        
        // pulling done and refitting
        while (App.ChunkMeshGenerator.TryDequeue(out var result))
        {
            App.GraphicEngine.EnqueueChunkRefit(result.job.GrxId, result.job.Model);
        }
    }
}