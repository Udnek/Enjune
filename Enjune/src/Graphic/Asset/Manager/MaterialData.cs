using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Data;

namespace Enjune.Graphic.Asset.Manager;

[StructLayout(LayoutKind.Explicit, Size = 16+4)]
public struct MaterialData
{
    [GraphicField(ShaderValuePrimitive.Vector4F, "color")]
    [FieldOffset(0)] public Vector4 Color;
    [GraphicField(ShaderValuePrimitive.Int, "texture_index")]
    [FieldOffset(16)] public int TextureIndex;
}