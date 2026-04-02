using System.Runtime.InteropServices;

namespace Enjune.Graphic.GraphicApi.Data;

[StructLayout(LayoutKind.Sequential)]
public struct WhiteVertexData(Vector3 position, TextureCoord textureCoord, TexId textureId)
{
    public Vector3 Position = position;
    public TextureCoord TextureCoord = textureCoord;
    public TexId TextureId = textureId;
}