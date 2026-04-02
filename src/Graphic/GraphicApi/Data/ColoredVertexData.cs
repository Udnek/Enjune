using System.Runtime.InteropServices;

namespace Enjune.Graphic.GraphicApi.Data;

[StructLayout(LayoutKind.Sequential)]
public struct ColoredVertexData(Position position, Color color, TextureCoord textureCoord, TexId textureId)
{
    public Position Position = position;
    public Color Color = color;
    public TextureCoord TextureCoord = textureCoord;
    public TexId TextureId = textureId;
}