using OpenTK.Mathematics;

namespace Minecraft2.Misc;

public class BaseTerrainGenerator : ITerrainGenerator
{
    public void Enqueue(Vector3i chunkPos)
    {
        throw new NotImplementedException();
    }

    public bool TryDequeue(out (Vector3i Pos, Chunk Chunk) result)
    {
        throw new NotImplementedException();
    }
}