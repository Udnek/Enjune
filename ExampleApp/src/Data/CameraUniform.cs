using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Attribute;
using OpenTK.Mathematics;

namespace ExampleApp.Data;

[StructLayout(LayoutKind.Explicit, Size = 16*4*2)]
public struct CameraUniform
{
    [GraphicField(ShaderValuePrimitive.Matrix4F, "view")]
    [FieldOffset(0)] public Matrix4 View;
    [GraphicField(ShaderValuePrimitive.Matrix4F, "proj")]
    [FieldOffset(16*4)] public Matrix4 Projection;
}