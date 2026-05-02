using System.Runtime.InteropServices;

namespace OpenGLApi.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SpotLightData(Matrix4 view, Matrix4 projection, Color color, Position position)
{
    public Matrix4 View = view; // 0+64 -> 64
    public Matrix4 Projection = projection; // 64+64 -> 128
    public Color Color = color; // 128+16 -> 144
    public Position Position = position; // 144+12 -> 156
    private int _padding0; // 156+4 -> 160 = 16*10
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LightsLengthData(int id)
{
    public int LightsLength = id; // 0+4 -> 4
    public Vector3 _padding; // 4+12 -> 16
}
