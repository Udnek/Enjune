using System.Diagnostics.CodeAnalysis;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Block;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Terrain;

public class ChunkWorld
{
    private readonly Dictionary<ChunkPos, Entity> _loadedChunks = [];
    private readonly HashSet<ChunkPos> _queuedForGeneration = [];
    private readonly App _app;

    public ChunkWorld(App app) => _app = app;

    /// <summary>
    /// Loads new chunk and adds to ecs world or just returns already loaded
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public void Load(ChunkPos pos)
    {
        // already loaded
        if (_loadedChunks.TryGetValue(pos, out var alreadyLoaded))
        {
            _app.World.ModifyEntityComponent<ChunkComponent>(alreadyLoaded, c => c with { ToBeUnloaded = false });
            return;
        }
        
        // already queued
        if (_queuedForGeneration.Contains(pos))
        {
            Logger.Info(this, $"{pos} already queued for generation");
            return;
        }

        _queuedForGeneration.Add(pos);
        _app.TerrainGenerator.Enqueue(pos, pos);
    }

    public void Unload(ChunkPos pos)
    {
        if (_loadedChunks.Remove(pos, out var chunk))
        {
            _app.World.RemoveEntityComponent<ChunkComponent>(chunk);
            Logger.Info(this, $"Unloading {pos} {chunk}");
        }
        else if (_queuedForGeneration.Remove(pos))
        {
            _app.TerrainGenerator.CancelJob(pos);
            Logger.Info(this, $"Dequeuing generation of {pos}");
        }
        else
            Logger.Info(this, $"Trying to unloaded {pos}, but already unloaded");
    }

    /// <summary>
    /// Tries to pull all generated chunks from TerrainGenerator
    /// </summary>
    public void PullLoading()
    {
        while (_app.TerrainGenerator.TryDequeue(out var pos, out var output))
        {
            if (!_queuedForGeneration.Remove(pos))
            {
                Logger.Warn(this, $"Got chunk that was not queued: {pos}");
                return;
            }
            
            var entity = _app.World.AddEntity(new Entity.Assembly()
                .AddComponent(new ChunkComponent
                {
                    Chunk = output,
                    Pos = pos
                }));
            _loadedChunks[pos] = entity;
        }
    }

    public bool TryGetChunk(ChunkPos pos, [MaybeNullWhen(false)] out Chunk chunk)
    {
        if (!_loadedChunks.TryGetValue(pos, out var entity))
        {
            chunk = null;
            return false;
        }

        var chunkComponent = _app.World.GetEntityComponent<ChunkComponent>(entity);
        if (chunkComponent is null)
        {
            Logger.Warn(this, $"chunk entity {pos} doesn't have {Logger.GetTypeName<ChunkComponent>()}!");
            chunk = null;
            return false;
        }
        
        chunk = chunkComponent.Value.Chunk;
        return true;
    }

    public bool TryGetBlock(BlockPos pos, [MaybeNullWhen(false)] out BlockType block)
    {
        var chunkPos = Chunk.ToChunkPos(pos);
        if (TryGetChunk(chunkPos, out var chunk))
        {
            block = chunk.GetByGlobal(pos);
            return true;
        }
        block = null;
        return false;
    }
    
    public bool TryTraceBlock(Vector3 absRayOrigin, Vector3 dir, float maxDistance, 
        [MaybeNullWhen(false)] out BlockPos hitBlockPos,
        [MaybeNullWhen(false)] out BlockType hitBlockType,
        out BlockFace? hitBlockFace)
    {
        hitBlockType = null;
        hitBlockPos = default;
        hitBlockFace = null;
        
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
        var block = chunk.GetByGlobal(absBlockOrigin);
        if (block.Collision != null && block.Collision.Value.ContainsPoint(absRayOrigin - absBlockOrigin))
        {
            hitBlockType = block;
            hitBlockPos = absBlockOrigin;
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

            block = chunk.GetByGlobal(absBlockPos);
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
            return true;
        }

        return false;
    } 
}