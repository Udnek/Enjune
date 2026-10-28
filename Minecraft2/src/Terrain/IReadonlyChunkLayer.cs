using System.Runtime.CompilerServices;

namespace Minecraft2.Terrain;

public interface IReadonlyChunkLayer<T>
{
    public bool IsDirty { get; } // has something changed. Used in mesh rendering
    public ChunkPos Position { get; }
    
    public BlockPos ToLocalPos(BlockPos globalPos);
    public BlockPos ToGlobalPos(BlockPos localPos);

    public T GetByGlobal(BlockPos globalPos);
    public T GetByLocal(BlockPos local);
    
    public bool ContainsGlobalBlockPos(BlockPos global);
    public bool ContainsLocalBlockPos(BlockPos local);

    public T SafeGetByGlobal(BlockPos global, T whenOutOfBounds);
    public T SafeGetByLocal(BlockPos local, T whenOutOfBounds);
}