using System.Runtime.CompilerServices;
using Enjune.Logger;

namespace Minecraft2.Terrain;

public abstract class AbstractChunkLayer<T> : IReadonlyChunkLayer<T>
{
    public int Version { get; private set; }
    public bool IsDirty { get; set; } = true;

    public ChunkPos Position { get; }

    public AbstractChunkLayer(ChunkPos position, T fillWith)
    {
        Position = position;
        Clear(fillWith);
    }

    public abstract void Clear(T fillWith);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockPos ToLocalPos(BlockPos globalPos) => globalPos - (Position * Chunk.Size);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockPos ToGlobalPos(BlockPos localPos) => localPos + (Position * Chunk.Size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetByGlobal(BlockPos globalPos) => GetByLocal(ToLocalPos(globalPos));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetByGlobal(BlockPos globalPos, T block) => SetByLocal(ToLocalPos(globalPos), block);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsGlobalBlockPos(BlockPos global) => ContainsLocalBlockPos(ToLocalPos(global));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsLocalBlockPos(BlockPos local)
    {
        return 0 <= local.X && local.X < Chunk.Size.X &&
               0 <= local.Y && local.Y < Chunk.Size.Y &&
               0 <= local.Z && local.Z < Chunk.Size.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract T GetByLocal(BlockPos local);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void SetByLocal(BlockPos local, T block);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T SafeGetByGlobal(BlockPos global, T whenOutOfBounds) 
        => SafeGetByLocal(ToLocalPos(global), whenOutOfBounds);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T SafeGetByLocal(BlockPos local, T whenOutOfBounds)
    {
        if (!ContainsLocalBlockPos(local))
            return whenOutOfBounds;
        return GetByLocal(local);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static int ToIndex(BlockPos pos) 
        => pos.X + 
           (Chunk.Size.X * pos.Z) + 
           (Chunk.Size.X * Chunk.Size.Z * pos.Y);
}