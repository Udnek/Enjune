using System.Runtime.InteropServices;

namespace Enjune.Graphic.GraphicApi;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexData(Position position, TextureCoord textureCoord)
{
    public Position Position = position;
    public TextureCoord TextureCoord = textureCoord;
}
