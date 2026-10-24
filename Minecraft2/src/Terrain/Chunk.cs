using System.Diagnostics;
using System.Runtime.CompilerServices;
using Enjune.Misc;
using Minecraft2.Block;

namespace Minecraft2.Terrain;

public class Chunk
{
    public static readonly ChunkPos Size = (32, 64, 32);

    static Chunk()
    {
        Trace.Assert(Size.X % 2 == 0);
        Trace.Assert(Size.Y % 2 == 0);
        Trace.Assert(Size.Z % 2 == 0);
    }
    
    private readonly BlockType[] _data = new BlockType[Size.X * Size.Y * Size.Z];
    public bool IsDirty = false;
    public readonly ChunkPos Position;

    public Chunk(ChunkPos position, BlockType fillWith)
    {
        Position = position;
        Array.Fill(_data, fillWith);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ChunkPos ToChunkPos(Vector3 pos) => 
        new((int) Math.Floor(pos.X / Size.X), 
            (int) Math.Floor(pos.Y / Size.Y), 
            (int) Math.Floor(pos.Z / Size.Z));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BlockPos ToGlobalBlockPos(Vector3 pos) => 
        new((int) Math.Floor(pos.X), 
            (int) Math.Floor(pos.Y), 
            (int) Math.Floor(pos.Z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockPos ToLocalPos(BlockPos globalPos) => globalPos - (Position * Size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockType GetByGlobal(BlockPos globalPos) => GetByLocal(ToLocalPos(globalPos));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetByGlobal(BlockPos globalPos, BlockType block) => SetByLocal(ToLocalPos(globalPos), block);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockType GetByLocal(BlockPos local) => _data[ToIndex(local)];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetByLocal(BlockPos local, BlockType block)
    {
        IsDirty = true;
        _data[ToIndex(local)] = block;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsGlobalBlockPos(BlockPos global) => ContainsLocalBlockPos(ToLocalPos(global));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsLocalBlockPos(BlockPos local)
    {
        return 0 <= local.X && local.X < Size.X &&
               0 <= local.Y && local.Y < Size.Y &&
               0 <= local.Z && local.Z < Size.Z;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockType SafeGetByGlobal(BlockPos global, BlockType whenOutOfBounds) 
        => SafeGetByLocal(ToLocalPos(global), whenOutOfBounds);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockType SafeGetByLocal(BlockPos local, BlockType whenOutOfBounds)
    {
        if (!ContainsLocalBlockPos(local))
            return whenOutOfBounds;
        return _data[ToIndex(local)];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ToIndex(BlockPos pos) => pos.X + (Size.X * pos.Z) + (Size.X * Size.Z * pos.Y);
}