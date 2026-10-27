using System.Diagnostics;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Graphic;
using Enjune.Graphic.Modeling;
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

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Retrieve<ChunkComponent, ChunkGraphicComponent>();
    }

    public override void OnUpdate()
    {
        _query.ForEach((_, ref chunkComp, ref chunkGrx) =>
        {
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
                InitialPos = chunk.Position,
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
            job.DownChunkVersion = job.DownChunk?.Version ?? 0;
            job.UpChunkVersion = job.UpChunk?.Version ?? 0;
            job.EastChunkVersion = job.EastChunk?.Version ?? 0;
            job.WestChunkVersion = job.WestChunk?.Version ?? 0;
            job.NorthChunkVersion = job.NorthChunk?.Version ?? 0;
            job.SouthChunkVersion = job.SouthChunk?.Version ?? 0;
            
            // mesh generation
            App.ChunkMeshGenerator.Enqueue(chunkGrx.GraphicId, job);
        });
        
        // pulling done and refitting
        while (App.ChunkMeshGenerator.TryDequeueDone(out Guid id, out var job))
        {
            if (job.TargetChunk.Position != job.InitialPos)
            {
                Logger.Warn(this, $"got chunk {job.TargetChunk.Position}, but initially {job.InitialPos}");
                continue;
            }
            if (!App.ChunkWorld.IsLoaded(job.TargetChunk.Position))
                continue;
            App.GraphicEngine.EnqueueChunkRefit(id, job.Model);
        }
    }
}