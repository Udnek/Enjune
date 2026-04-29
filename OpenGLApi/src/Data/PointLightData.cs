using System.Runtime.InteropServices;

namespace OpenGLApi.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PointLightData(Position position, Color color)
{
    public Position Position = position; // 0+12 -> 12
    public float _padding; // 12+4 -> 16
    public Color Color = color; // 16+16 -> 32
}