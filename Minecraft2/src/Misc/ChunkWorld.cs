using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using OpenTK.Mathematics;

namespace Minecraft2.Misc;

public class ChunkWorld
{
    private readonly Dictionary<Vector3i, Entity> _loadedChunks = [];
    private readonly FastNoiseLite _noise = new();
    private readonly World _ecsWorld;

    public ChunkWorld(World ecsWorld)
    {
        _ecsWorld = ecsWorld;
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    }

    /// <summary>
    /// Loads new chunk and adds to ecs world or just returns already loaded
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="wasAlreadyLoaded"></param>
    /// <returns></returns>
    public Entity Load(Vector3i pos, out bool wasAlreadyLoaded)
    {
        if (_loadedChunks.TryGetValue(pos, out var alreadyLoaded))
        {
            _ecsWorld.ModifyEntityComponent<ChunkComponent>(alreadyLoaded, c => c with { ToBeUnloaded = false });
            wasAlreadyLoaded = true;
            return alreadyLoaded;
        }

        wasAlreadyLoaded = false;
        var entity = _ecsWorld.AddEntity(new Entity.Assembly()
            .AddComponent(new ChunkComponent
            {
                Chunk = GenerateChunk(pos),
                Pos = pos
            }));
        _loadedChunks[pos] = entity;
        Logger.Highlight(this, $"Loading fresh {pos} {entity}");
        return entity;
    }

    private Chunk GenerateChunk(Vector3i chunkPos)
    {
        var chunk = new Chunk();
        for (int blockX = 0; blockX < Chunk.Size.X; blockX++)
        {
            for (int blockZ = 0; blockZ < Chunk.Size.Z; blockZ++)
            {
                var y = _noise.GetNoise(chunkPos.X*Chunk.Size.X +blockX, chunkPos.Y*Chunk.Size.Z +blockZ);
                var height = (int) Math.Clamp((y+1f)/2f*Chunk.Size.Y, 0, Chunk.Size.Y);
                for (int i = 0; i < height; i++)
                {
                    chunk[new(blockX, i, blockZ)] = true;
                }
            }
        }

        return chunk;
    }

    public void Unload(Vector3i pos)
    {
        if (_loadedChunks.Remove(pos, out var chunk))
        {
            _ecsWorld.RemoveEntityComponent<ChunkComponent>(chunk);
            Logger.Highlight(this, $"Unloading {pos} {chunk}");
        }
        else
            Logger.Highlight(this, $"Trying to unloaded {pos}, but already unloaded");
    }
}