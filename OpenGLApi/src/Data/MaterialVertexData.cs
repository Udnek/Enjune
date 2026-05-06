using System.Runtime.InteropServices;

namespace OpenGLApi.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexData(Position position, Vector2 texturePos, Normal normal)
{
    public Position Position = position;
    public Vector2 TexturePos = texturePos;
    public Normal Normal = normal;
}
