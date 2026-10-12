using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Data;
using OpenTK.Mathematics;

namespace ExampleApp;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PerVertex
{
    [GraphicField(ValuePrimitive.Vector3F, "pos")]
    public Vector3 Pos;
    [GraphicField(ValuePrimitive.Vector4F, "color")]
    public Vector4 Color;
}