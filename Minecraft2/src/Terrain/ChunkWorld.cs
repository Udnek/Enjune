using System.Diagnostics.CodeAnalysis;
using Enjune.Attribute;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Block;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Terrain;

[LogParams(logCallingMethod:true)]
public class ChunkWorld
{
    private readonly Dictionary<ChunkPos, Entity> _loadedChunks = [];
    private readonly HashSet<ChunkPos> _queuedForGeneration = [];
    private readonly App _app;

    public ChunkWorld(App app) => _app = app;

    /// <summary>
    /// Queses chunk for loading if not already
    /// </summary>
    /// <returns></returns>
    public void QueueLoading(ChunkPos chunkPos)
    {
        // already loaded
        if (_loadedChunks.TryGetValue(chunkPos, out var alreadyLoaded))
        {
            _app.World.ModifyEntityComponent<ChunkComponent>(alreadyLoaded, c => c with { ToBeUnloaded = false });
            return;
        }
        
        // already queued
        if (IsQueued(chunkPos))
            return;

        var chunk = new Chunk(chunkPos, new ChunkLayer<BlockType>(chunkPos, _app.Blocks.Air));
        _queuedForGeneration.Add(chunkPos);
        _app.TerrainGenerator.Enqueue(chunkPos, chunk);
    }

    public void Unload(ChunkPos chunkPos)
    {
        if (_loadedChunks.Remove(chunkPos, out var chunkEntity))
        {
            var chunkComp = _app.World.GetEntityComponent<ChunkComponent>(chunkEntity);
            if (chunkComp is null)
            {
                Logger.Warn(this, $"Chunk {chunkPos} doesn't have {Logger.GetTypeName<ChunkComponent>()}");
                return;
            }
            _app.World.RemoveEntityComponent<ChunkComponent>(chunkEntity);
            //_app.ChunkPool.Recycle(chunkComp.Value.Chunk);
            Logger.Info(this, $"Unloading {chunkPos} {chunkEntity}");
        }
        else if (_queuedForGeneration.Remove(chunkPos))
        {
            _app.TerrainGenerator.EnqueueJobCancel(chunkPos);
            Logger.Highlight(this, $"Dequeuing generation of {chunkPos}");
        }
        else
            Logger.Warn(this, $"Trying to unloaded {chunkPos}, but already unloaded");
    }

    /// <summary>
    /// Tries to pull all generated chunks from TerrainGenerator
    /// </summary>
    public void PullFromGenerator()
    {
        // done
        while (_app.TerrainGenerator.TryDequeueDone(out var pos, out var chunk))
        {
            if (!_queuedForGeneration.Remove(pos))
            {
                Logger.Warn(this, $"Got chunk that was not queued: {pos}");
                return;
            }
            
            var entity = _app.World.AddEntity(new Entity.Assembly()
                .AddComponent(new ChunkComponent { Chunk = chunk }));
            _loadedChunks[pos] = entity;
        }
        // cancelled
        while (_app.TerrainGenerator.TryDequeueCancelled(out var pos, out var chunk))
        {
            
        }
    }

    public bool IsQueued(ChunkPos chunkPos) => _queuedForGeneration.Contains(chunkPos);

    public bool IsLoaded(ChunkPos chunkPos) => _loadedChunks.ContainsKey(chunkPos);

    public bool TryGetChunkEntity(ChunkPos chunkPos, [MaybeNullWhen(false)] out Entity chunk) 
        => _loadedChunks.TryGetValue(chunkPos, out chunk);

    public bool TryGetChunk(ChunkPos chunkPos, [MaybeNullWhen(false)] out Chunk chunk)
    {
        if (!TryGetChunkEntity(chunkPos, out var entity))
        {
            chunk = null;
            return false;
        }

        var chunkComponent = _app.World.GetEntityComponent<ChunkComponent>(entity);
        if (chunkComponent is null)
        {
            Logger.Warn(this, $"chunk entity {chunkPos} doesn't have {Logger.GetTypeName<ChunkComponent>()}!");
            chunk = null;
            return false;
        }
        
        chunk = chunkComponent.Value.Chunk;
        return true;
    }
    
    public bool TryTraceBlock(Vector3 absRayOrigin, Vector3 dir, float maxDistance, 
        [MaybeNullWhen(false)] out BlockPos hitBlockPos,
        [MaybeNullWhen(false)] out BlockType hitBlockType,
        [MaybeNullWhen(false)] out Chunk hitChunk,
        out BlockFace? hitBlockFace)
    {
        hitBlockType = null;
        hitBlockPos = default;
        hitBlockFace = null;
        hitChunk = null;
        
        if (dir.LengthSquared == 0)
        {
            Logger.Error(this, "given direction is 0");
            return false;
        }

        if (maxDistance <= 0)
            return false;
        
        if (!TryGetChunk(Chunk.ToChunkPos(absRayOrigin), out var chunk))
            return false;
        
        var absBlockOrigin = Chunk.ToGlobalBlockPos(absRayOrigin);
        var block = chunk.BlockLayer.GetByGlobal(absBlockOrigin);
        if (block.Collision != null && block.Collision.Value.ContainsPoint(absRayOrigin - absBlockOrigin))
        {
            hitBlockType = block;
            hitBlockPos = absBlockOrigin;
            hitChunk = chunk;
            return true;
        }
        
        var absBlockPos = absBlockOrigin;
        for (int i = 0; i < maxDistance; i++)
        {
            MathUtils.RayAABBIntersectionNearFar(absRayOrigin, dir,
                absBlockPos,
                absBlockPos + Vector3.One,
                out _, out var far);

            var stepDir = BlockPos.Zero;
            if (far.X < far.Y && far.X < far.Z)      // x is min
                stepDir.X = Math.Sign(dir.X);
            else if (far.Y < far.X && far.Y < far.Z) // y is min
                stepDir.Y = Math.Sign(dir.Y);
            else                                     // z is min
                stepDir.Z = Math.Sign(dir.Z);

            absBlockPos += stepDir;
            
            if (!chunk.ContainsGlobalBlockPos(absBlockPos)) // we are beyond current chunk
            {
                if (!TryGetChunk(Chunk.ToChunkPos(absBlockPos), out chunk))
                    return false;
            }

            block = chunk.BlockLayer.GetByGlobal(absBlockPos);
            if (block.Collision == null) continue;
            var collision = block.Collision.Value;
            if (!MathUtils.RayEntersAABB(
                    absRayOrigin, dir, absBlockPos + collision.Min, absBlockPos + collision.Max,
                    out var enterPos))
            {
                continue;
            }
            
            if ((enterPos - absBlockOrigin).LengthSquared > maxDistance * maxDistance)
                return false;

            hitBlockFace = BlockFace.FromDirection(stepDir).Opposite();
            hitBlockPos = absBlockPos;
            hitBlockType = block;
            hitChunk = chunk;
            return true;
        }

        return false;
    } 
}