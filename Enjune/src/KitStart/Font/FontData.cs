using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Attribute;
using Enjune.Graphic.Modeling;

namespace Enjune.KitStart.Font;

public static class FontData
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct PerPrimitive(Color color)
    {
        [GraphicField(ShaderValuePrimitive.Vector4F, "color")]
        [FieldOffset(0)] public Vector4 Color = color;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex : IPositionedVertex, ITexturedVertex
    {
        [field: GraphicField(ShaderValuePrimitive.Vector3F, "position")]
        public Position Position { get; set; }
        [field: GraphicField(ShaderValuePrimitive.Vector2F, "uv")]
        public TexturePos Uv { get; set; }
    }
}