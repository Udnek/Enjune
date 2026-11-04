using System.Runtime.CompilerServices;
using Enjune.Logger;
using Enjune.Misc;
using Minecraft2.Block;

namespace Minecraft2.Terrain;

public class ArrayChunkLayer<T> : AbstractChunkLayer<T>
{
    private readonly T[] _data = new T[Chunk.Size.X * Chunk.Size.Y * Chunk.Size.Z];

    public ArrayChunkLayer(ChunkPos position, T fillWith) : base(position, fillWith) { }

    public override void Clear(T fillWith) => Array.Fill(_data, fillWith);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override T GetByLocal(BlockPos local)
    {
        if (!ContainsLocalBlockPos(local))
        {
            Logger.Error(this, $"can not get: local {local} is out of bounds");
            return default!;
        }
        return _data[ToIndex(local)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SetByLocal(BlockPos local, T block)
    {
        if (!ContainsLocalBlockPos(local))
        {
            Logger.Error(this, $"can not set: local {local} is out of bounds");
            return;
        }
        IsDirty = true;
        _data[ToIndex(local)] = block;
    }
}