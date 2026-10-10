using System.Diagnostics;
using System.Runtime.CompilerServices;
using Enjune.Attribute;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Minecraft2.Misc;

public class Chunk
{
    public static readonly ChunkPos Size = (32, 64, 32);

    static Chunk()
    {
        Trace.Assert(Size.X % 2 == 0);
        Trace.Assert(Size.Y % 2 == 0);
        Trace.Assert(Size.Z % 2 == 0);
    }
    
    private readonly bool[] _data = new bool[Size.X * Size.Y * Size.Z];
    public bool IsDirty = false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ChunkPos ToChunkPos(Vector3 pos) => 
        new((int) Math.Floor(pos.X / Size.X), 
            (int) Math.Floor(pos.Y / Size.Y), 
            (int) Math.Floor(pos.Z / Size.Z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ToIndex(ChunkPos pos) => pos.X + (Size.X * pos.Z) + (Size.X * Size.Z * pos.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SafeGet(ChunkPos pos, bool whenOutOfBounds)
    {
        if (pos.X < 0 || Size.X <= pos.X ||
            pos.Y < 0 || Size.Y <= pos.Y || 
            pos.Z < 0 || Size.Z <= pos.Z) return whenOutOfBounds;
        return this[pos];
    }
    
    public bool this[ChunkPos pos]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _data[ToIndex(pos)];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            IsDirty = true;
            _data[ToIndex(pos)] = value;
        }
    }
}