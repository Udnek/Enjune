using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Data;
using Enjune.Graphic.Modeling;
using OpenTK.Mathematics;

namespace ExampleApp.Data;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PerVertex() : IPositionedVertex, ITexturedVertex
{
    [field: GraphicField(ShaderValuePrimitive.Vector3F, "pos")]
    public Vector3 Position { get; set; }
    [GraphicField(ShaderValuePrimitive.Vector4F, "color")]
    public Vector4 Color = Vector4.One;
    [field: GraphicField(ShaderValuePrimitive.Vector2F, "uv")]
    public Vector2 Uv { get; set; }
}