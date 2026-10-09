using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

namespace Minecraft2.Misc;

public interface ITerrainGenerator
{
    /// <summary>
    /// Enqueues chunk to be generated
    /// </summary>
    /// <param name="chunkPos"></param>
    public void Enqueue(Vector3i chunkPos);
    
    /// <summary>
    /// Returns generated chunks in FIFO order
    /// </summary>
    /// <param name="result"></param>
    /// <returns>True if result queues is not empty</returns>
    public bool TryDequeue( [MaybeNullWhen(false)] out (Vector3i Pos, Chunk Chunk) result);
}