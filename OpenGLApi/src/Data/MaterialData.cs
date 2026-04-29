using System.Runtime.InteropServices;

namespace OpenGLApi.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialData(Color color, TexId textureId)
{
    public Vector4 Color = color; // 16
    public int TextureId = textureId; // 16 -> 20
    private Vector3 padding; // 20 -> 32
}