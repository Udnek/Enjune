using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Data;
using OpenTK.Mathematics;

namespace ExampleApp;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct TimeUniform
{
    [GraphicField(ShaderValuePrimitive.Float, "sec")]
    [FieldOffset(0)] public float Seconds;
}