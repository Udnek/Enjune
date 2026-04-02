using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace Enjune.Graphic.GraphicApi;

[StructLayout(LayoutKind.Sequential)]
public struct ColoredVertexData
{
    private Vector3 position;
    private Color color;
    private TextureCoord textureCoord;
    private TexId textureId;
}