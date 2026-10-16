using System.Runtime.InteropServices;

namespace Enjune.File.ModelReader;

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct MaterialPerPrimitive(int matIdx)
{
    [FieldOffset(0)] public int MaterialIndex = matIdx;
}