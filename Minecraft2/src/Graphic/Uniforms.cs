using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Attribute;

namespace Minecraft2.Graphic;

public static class Uniforms
{
    [StructLayout(LayoutKind.Explicit, Size = 16*4*2)]
    public struct Camera
    {
        [GraphicField(ShaderValuePrimitive.Matrix4F, "view")]
        [FieldOffset(0)] public Matrix4 View;
        [GraphicField(ShaderValuePrimitive.Matrix4F, "proj")]
        [FieldOffset(16*4)] public Matrix4 Projection;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 16*4)]
    public struct Model
    {
        [GraphicField(ShaderValuePrimitive.Matrix4F, "mat")]
        [FieldOffset(0)] public Matrix4 Matrix;
    }
}