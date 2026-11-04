using System.Runtime.CompilerServices;
using Enjune.Logger;
using Enjune.Misc;

namespace Minecraft2.Terrain;

public class PaletteChunkLayer<T> : AbstractChunkLayer<T> where T : IEquatable<T>
{
    private static readonly int Length = Chunk.Size.X * Chunk.Size.Y * Chunk.Size.Z;
    private readonly PaletteMappedBuffer<T> _data = new(Length, 4);
    
    public PaletteChunkLayer(ChunkPos position, T fillWith) : base(position, fillWith) { }

    public override void Clear(T fillWith)
    {
        for (int i = 0; i < Length; i++) 
            _data[i] = fillWith;
    }

    public override T GetByLocal(BlockPos local)
    {
        if (!ContainsLocalBlockPos(local))
        {
            Logger.Error(this, $"can not get: local {local} is out of bounds");
            return default!;
        }
        return _data[ToIndex(local)];
    }

    public override void SetByLocal(BlockPos local, T block)
    {
        if (!ContainsLocalBlockPos(local))
        {
            Logger.Error(this, $"can not set: local {local} is out of bounds");
            return;
        }
        _data[ToIndex(local)] = block;
    }
}