using Enjune.Misc;
using OpenTK.Mathematics;

namespace Minecraft2.Misc;

public class ChunkWorld
{
    private readonly Dictionary<Vector3i, Chunk> _loadedChunks = [];
    private readonly FastNoiseLite _noise = new();

    public ChunkWorld()
    {
        _noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    }
    
    public Chunk Load(Vector3i pos, out bool wasAlreadyLoaded)
    {
        if (_loadedChunks.TryGetValue(pos, out var loadedChunk))
        {
            loadedChunk.MarkedUnloaded = false;
            wasAlreadyLoaded = true;
            return loadedChunk;
        }

        wasAlreadyLoaded = false;
        Logger.Highlight(this, $"Loading {pos}");
        var chunk = GenerateChunk(pos);
        _loadedChunks[pos] = chunk;
        return chunk;
    }

    private Chunk GenerateChunk(Vector3i chunkPos)
    {
        var chunk = new Chunk(chunkPos);
        for (int blockX = 0; blockX < Chunk.Size.X; blockX++)
        {
            for (int blockZ = 0; blockZ < Chunk.Size.Z; blockZ++)
            {
                var y = _noise.GetNoise(chunkPos.X*Chunk.Size.X +blockX, chunkPos.Y*Chunk.Size.Z +blockZ);
                var height = (int) Math.Clamp(y*Chunk.Size.Y , 0, Chunk.Size.Y);
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
            chunk.MarkedUnloaded = true;
            Logger.Highlight(this, $"Unloading {pos}");
        }
        else
            Logger.Highlight(this, $"Trying to unloaded {pos}, but already unloaded");
    }
}