using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Data;
using OpenTK.Mathematics;

namespace ExampleApp;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct PerPrimitive
{
    [GraphicField(ShaderValuePrimitive.Vector4F, "color")]
    [FieldOffset(0)] public Vector4 Color;
}