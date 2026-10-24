using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Attribute;
using Enjune.Graphic.Modeling;

namespace Minecraft2.Graphic;

public static class GraphicScreenApi
{
    public static readonly string VertexShader = 
        """
        vec4 process(Vertex vertex, out Varyings varyings) {
            varyings.uv = vertex.uv;
            return vec4(vertex.pos, 0, 1);
        }
        """;

    public static readonly string FragmentShader =
        """
        vec4 process(Varyings varyings, PerPrimitive perPrim) {
            return texture(screen_tex, varyings.uv);
            // vec2 texSize = textureSize(screen_tex, 0);
            // vec2 center = varyings.uv;
            // vec4 sum = vec4(0f);
            // float r = 3f;
            // for (float i = -r; i <= r; i++){
            //     for (float j = -r; j <= r; j++){
            //         sum += texture(screen_tex, center + vec2(i, j)/texSize);
            //     }
            // }
            // return sum / ((2*r+1)*(2*r+1));
        }
        """;
    
    public struct Varyings()
    {
        [field: GraphicField(ShaderValuePrimitive.Vector2F, "uv")]
        public Vector2 Uv;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex() : ITexturedVertex
    {
        [field: GraphicField(ShaderValuePrimitive.Vector2F, "pos")]
        public Vector2 Position { get; set; }
        [field: GraphicField(ShaderValuePrimitive.Vector2F, "uv")]
        public Vector2 Uv { get; set; }
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Primitive
    {
        [field: GraphicField(ShaderValuePrimitive.Int, "nothing")]
        [FieldOffset(0)] public int Nothing;
    }
}