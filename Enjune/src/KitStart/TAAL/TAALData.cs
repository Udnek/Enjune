using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Attribute;
using Enjune.Graphic.Modeling;

namespace Enjune.KitStart.TAAL;

public static class TAALData
{
    /// <summary>
    /// Should be put int UniformArray to be read using PerPrimitive's mat_idx
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16+4)]
    public record struct Material
    {
        public Material(int texIdx) => TextureIndex = texIdx;
        
        [GraphicField(ShaderValuePrimitive.Vector4F, "color")]
        [FieldOffset(0)] public Vector4 Color = Vector4.One;
        [GraphicField(ShaderValuePrimitive.Int, "tex_idx")]
        [FieldOffset(16)] public int TextureIndex;
    }
    
    /// <summary>
    /// PerPrimitive that stores material index
    /// </summary>
    /// <param name="matIdx"></param>
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct PerPrimitive(int matIdx)
    {
        [GraphicField(ShaderValuePrimitive.Int, "mat_idx")]
        [FieldOffset(0)] public int MaterialIndex = matIdx;
    }
    
    /// <summary>
    /// Vertex data
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex : IPositionedVertex, ITexturedVertex
    {
        [field: GraphicField(ShaderValuePrimitive.Vector3F, "position")]
        public Position Position { get; set; }
        [field: GraphicField(ShaderValuePrimitive.Vector2F, "uv")]
        public TexturePos Uv { get; set; }
    }
}