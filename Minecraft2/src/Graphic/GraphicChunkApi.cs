using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Attribute;
using Enjune.Graphic.Modeling;

namespace Minecraft2.Graphic;

public static class GraphicChunkApi
{
    public static readonly string VertexShader = 
        """
        vec4 process(Vertex vertex, out Varyings varyings) {
            varyings.color = vertex.color;
            varyings.uv = vertex.uv;
            return camera.proj * camera.view * model.mat * vec4(vertex.pos, 1);
        }
        """;

    public static readonly string FragmentShader =
        """
        vec4 process(Varyings varyings, PerPrimitive perPrim) {
            return vec4(varyings.color * perPrim.color, 1) * 
                    texture(block_tex, vec3(varyings.uv, perPrim.tex_idx));
        }
        """;
    
    public struct Varyings()
    {
        [GraphicField(ShaderValuePrimitive.Vector3F, "color")]
        public Vector3 Color;
        [field: GraphicField(ShaderValuePrimitive.Vector2F, "uv")]
        public Vector2 Uv;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex() : IPositionedVertex<uint>, ITexturedVertex
    {
        [field: GraphicField(ShaderValuePrimitive.UInt, "pos")]
        public uint Position { get; set; }
        [GraphicField(ShaderValuePrimitive.UInt, "color")]
        public uint Color = uint.MaxValue; // white
        [field: GraphicField(ShaderValuePrimitive.Vector2F, "uv")]
        public Vector2 Uv { get; set; }
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct Primitive()
    {
        [GraphicField(ShaderValuePrimitive.UInt, "color")]
        [FieldOffset(0)] public uint Color = uint.MaxValue; // white
        [GraphicField(ShaderValuePrimitive.Int, "tex_idx")]
        [FieldOffset(12)] public required int TextureIndex;
    }
}