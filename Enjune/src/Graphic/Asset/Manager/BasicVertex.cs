using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Data;
using Enjune.Graphic.Modeling;

namespace Enjune.File.ModelReader;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BasicVertex : IPositionedVertex, ITexturedVertex
{
    [field: GraphicField(ShaderValuePrimitive.Vector3F, "position")]
    public Position Position { get; set; }
    [field: GraphicField(ShaderValuePrimitive.Vector2F, "uv")]
    public TexturePos Uv { get; set; }
}