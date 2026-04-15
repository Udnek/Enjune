using System.Runtime.InteropServices;

namespace Enjune.Graphic.GraphicApi.Vertex.Material;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialVertexData(Position position, TextureCoord textureCoord)
{
    public Position Position = position;
    public TextureCoord TextureCoord = textureCoord;
}
