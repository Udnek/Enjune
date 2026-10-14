using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Data;
using OpenTK.Mathematics;

namespace ExampleApp;

[StructLayout(LayoutKind.Explicit, Size = 12+16)]
public struct PerVertex
{
    [GraphicField(ShaderValuePrimitive.Vector3F, "pos")]
    [FieldOffset(0)] public Vector3 Pos;
    [GraphicField(ShaderValuePrimitive.Vector4F, "color")]
    [FieldOffset(12)] public Vector4 Color;
}