using System.Runtime.InteropServices;

namespace Enjune.Graphic.GraphicApi.Vertex.Colored;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ColoredVertexData(Position position, Color color)
{
    public Position Position = position;
    public Color Color = color;
}