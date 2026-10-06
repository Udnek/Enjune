using System.Diagnostics;
using Enjune.Attribute;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Minecraft2.Misc;

[LogParams(method: LogParamsAttribute.Method.ToString)]
public class Chunk
{
    public static readonly Vector3i Size = (32, 64, 32);

    static Chunk()
    {
        Trace.Assert(Size.X % 2 == 0, "Size.X % 2 == 0");
        Trace.Assert(Size.Y % 2 == 0, "Size.Y % 2 == 0");
        Trace.Assert(Size.Z % 2 == 0, "Size.Z % 2 == 0");
    }
    
    private readonly bool[] _data = new bool[Size.X * Size.Y * Size.Z];
    public bool IsDirty = false;
    public readonly Vector3i Position;

    public Chunk(Vector3i pos)
    {
        Position = pos;
    }

    private static int ToIndex(Vector3i pos) => pos.X + (Size.X * pos.Z) + (Size.X * Size.Z * pos.Y);

    public bool SafeGet(Vector3i pos, bool whenOutOfBounds)
    {
        var index = ToIndex(pos);
        if (0 <= index && index < _data.Length) 
            return _data[index];
        return whenOutOfBounds;
    }
    
    public bool this[Vector3i pos]
    {
        get => _data[ToIndex(pos)];
        set
        {
            IsDirty = true;
            _data[ToIndex(pos)] = value;
        }
    }
    
    public override string ToString() => $"{nameof(Chunk)}[{Position}]";
}