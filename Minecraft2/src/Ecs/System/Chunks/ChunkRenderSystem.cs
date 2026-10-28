using System.Diagnostics;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Graphic;
using Enjune.Graphic.Modeling;
using Enjune.Logger;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System.Misc;
using Minecraft2.Misc;
using Minecraft2.Terrain;
using OpenTK.Mathematics;

namespace Minecraft2.Ecs.System.Chunks;

public class ChunkRenderSystem : AppSystem
{
    private Query<ChunkComponent, ChunkGraphicComponent> _query = null!;
    private World _world = null!;

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Retrieve<ChunkComponent, ChunkGraphicComponent>();
        _world = world;
    }

    public override void OnUpdate()
    {
        _query.ForEach((_, ref chunkComp, ref chunkGrx) =>
        {
            if (chunkComp.ToBeUnloaded) 
                return;
            
            if (chunkGrx.PendingNeighbors > 0) 
                return;
            
            var chunk = chunkComp.Chunk;
            if (!(chunk.BlockLayer.IsDirty || chunkGrx.BlockSizeChanged)) 
                return;
            
            chunk.BlockLayer.IsDirty = false;
            chunkGrx.BlockSizeChanged = false;

            var pos = chunk.Position;

            Chunk? GetChunk(ChunkPos offset)
            {
                App.ChunkWorld.TryGetChunk(pos + offset, out var gotChunk);
                return gotChunk;
            }

            var job = new ChunkMeshGenerator.Job
            {
                TargetChunk = chunk,
                DownChunk = GetChunk((0, -1, 0)),
                UpChunk = GetChunk((0, 1, 0)),
                NorthChunk = GetChunk((0, 0, 1)),
                SouthChunk = GetChunk((0, 0, -1)),
                EastChunk = GetChunk((1, 0, 0)),
                WestChunk = GetChunk((-1, 0, 0)),
                Model = chunkGrx.Model,
                BlockSize = chunkGrx.BlockSize
            };

            // mesh generation
            App.ChunkMeshGenerator.Enqueue(chunkGrx.GraphicId, job);
        });
        
        // pulling done and refitting
        while (App.ChunkMeshGenerator.TryDequeueDone(out Guid id, out var job))
        {
            var pos = job.TargetChunk.Position;
            if (!App.ChunkWorld.TryGetChunkEntity(pos, out var entity))
            {
                Logger.Warn(this, $"Created mesh for unloaded chunk: {pos}");
                continue;
            }

            var grxComp = _world.GetEntityComponent<ChunkGraphicComponent>(entity)!.Value;
            if (grxComp.GraphicId != id)
            {
                Logger.Warn(this, $"Created mesh for chunk {entity} with {id} but real id is {grxComp.GraphicId}");
                continue;
            }
            App.GraphicEngine.EnqueueChunkRefit(id, job.Model);
        }
    }
}