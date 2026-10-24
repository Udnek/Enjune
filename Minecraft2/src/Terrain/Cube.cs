using System.Runtime.CompilerServices;

namespace Minecraft2.Terrain;

public struct Cube<TVert>
{
    public TVert V000; // bot
    public TVert V100;
    public TVert V101;
    public TVert V001;
    public TVert V010; // top
    public TVert V110;
    public TVert V111;
    public TVert V011;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Vertex(bool x, bool y, bool z)
    {
        
    }
}