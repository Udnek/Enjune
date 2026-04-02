using System.Runtime.InteropServices;

namespace Enjune.Graphic.GraphicApi;

[StructLayout(LayoutKind.Sequential)]
public struct WhiteVertexData
{
    private Vector3 position;
    private TextureCoord textureCoord;
    private TexId textureId;
}