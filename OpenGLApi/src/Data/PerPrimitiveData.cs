using System.Runtime.InteropServices;

namespace OpenGLApi.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PerPrimitiveData(MatId matId, Color color)
{
    public Color Color = color; // 0+16 -> 16;
    public MatId MaterialId = matId; // 16+4 -> 20;
    private Vector3 _padding; // 20 -> 32
}