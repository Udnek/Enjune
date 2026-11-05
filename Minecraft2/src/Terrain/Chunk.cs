using System.Diagnostics;
using System.Runtime.CompilerServices;
using Enjune.Misc;
using Minecraft2.Block;

namespace Minecraft2.Terrain;

public class Chunk : IReadonlyChunk
{
    public static readonly ChunkPos Size = (16, 128, 16);

    static Chunk()
    {
        Trace.Assert(Size.X % 2 == 0);
        Trace.Assert(Size.Y % 2 == 0);
        Trace.Assert(Size.Z % 2 == 0);
    }

    public ChunkPos Position { get; }

    IReadonlyChunkLayer<BlockState> IReadonlyChunk.BlockLayer => BlockLayer;
    public readonly PaletteChunkLayer<BlockState> BlockLayer;
    
    public Chunk(ChunkPos pos, PaletteChunkLayer<BlockState> blockLayer)
    {
        BlockLayer = blockLayer;
        Position = pos;
    }

    public void Clear(BlockState air) => BlockLayer.Clear(air);

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
    public bool ContainsGlobalBlockPos(BlockPos global) => ContainsLocalBlockPos(ToLocalPos(global));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockPos ToLocalPos(BlockPos globalPos) => globalPos - (Position * Size);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsLocalBlockPos(BlockPos local)
    {
        return 0 <= local.X && local.X < Size.X &&
               0 <= local.Y && local.Y < Size.Y &&
               0 <= local.Z && local.Z < Size.Z;
    }
}