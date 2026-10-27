using System.Runtime.CompilerServices;
using Enjune.Misc;
using Minecraft2.Block;

namespace Minecraft2.Terrain;

public class ChunkLayer<T> : IReadonlyChunkLayer<T>
{
    private readonly T[] _data = new T[Chunk.Size.X * Chunk.Size.Y * Chunk.Size.Z];

    public int Version { get; private set; }
    public bool IsDirty { get; set; } = false;

    public ChunkPos Position
    {
        get;
        set
        {
            field = value;
            Version++;
        }
    }

    public ChunkLayer(ChunkPos position, T fillWith)
    {
        Position = position;
        Clear(fillWith);
    }

    public void Clear(T fillWith)
    {
        Array.Fill(_data, fillWith);
        IsDirty = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockPos ToLocalPos(BlockPos globalPos) => globalPos - (Position * Chunk.Size);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockPos ToGlobalPos(BlockPos localPos) => localPos + (Position * Chunk.Size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetByGlobal(BlockPos globalPos) => GetByLocal(ToLocalPos(globalPos));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetByGlobal(BlockPos globalPos, T block) => SetByLocal(ToLocalPos(globalPos), block);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetByLocal(BlockPos local)
    {
        if (!ContainsLocalBlockPos(local))
        {
            Logger.Error(this, $"can not get: local {local} is out of bounds");
            return default!;
        }
        return _data[ToIndex(local)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetByLocal(BlockPos local, T block)
    {
        if (!ContainsLocalBlockPos(local))
        {
            Logger.Error(this, $"can not set: local {local} is out of bounds");
            return;
        }
        IsDirty = true;
        _data[ToIndex(local)] = block;
    }

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
    public T SafeGetByGlobal(BlockPos global, T whenOutOfBounds) 
        => SafeGetByLocal(ToLocalPos(global), whenOutOfBounds);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T SafeGetByLocal(BlockPos local, T whenOutOfBounds)
    {
        if (!ContainsLocalBlockPos(local))
            return whenOutOfBounds;
        return _data[ToIndex(local)];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ToIndex(BlockPos pos) 
        => pos.X + 
           (Chunk.Size.X * pos.Z) + 
           (Chunk.Size.X * Chunk.Size.Z * pos.Y);
}