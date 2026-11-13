using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Attribute;
using Enjune.Graphic.Modeling;

namespace Minecraft2.Graphic;

public static class GraphicChunkApi
{
    public static readonly string VertexShader = 
        """
        vec4 unpack(uint p) {
            float x = float((p >> 0) & 0xFFu);
            float y = float((p >> 8) & 0xFFu);
            float z = float((p >> 16) & 0xFFu);
            float w = float((p >> 24) & 0xFFu);
            return vec4(x, y, z, w);
        }
        
        vec4 process(Vertex vertex, out Varyings varyings) {
            varyings.color = unpack(vertex.color).xyz / 255f;
            varyings.uv = vertex.uv;
            vec4 pos = unpack(vertex.pos);
            pos.w = 1;
            return camera.proj * camera.view * model.mat * pos;
        }
        """;

    public static readonly string FragmentShader =
        """
        vec3 unpack(uint p) {
            float x = float((p >> 0) & 0xFFu);
            float y = float((p >> 8) & 0xFFu);
            float z = float((p >> 16) & 0xFFu);
            return vec3(x, y, z);
        }
        
        vec4 process(Varyings varyings, PerPrimitive perPrim) {
            return vec4(varyings.color * unpack(perPrim.color)/255f, 1) * 
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
        [FieldOffset(4)] public required int TextureIndex;
    }
}