using System.Runtime.InteropServices;

namespace OpenGLApi.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ColoredVertexData(Position position, Color color)
{
    public Position Position = position;
    public Color Color = color;
}