using System.Runtime.InteropServices;

namespace Enjune.Graphic.GraphicApi.Vertex.Material;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialVertexData(Position position, TextureCoord textureCoord, Vector3 normal)
{
    public Position Position = position;
    public TextureCoord TextureCoord = textureCoord;
    public Vector3 Normal = normal;
}
