using System.Runtime.InteropServices;
using Enjune.Graphic.Api.Attribute;
using Enjune.Graphic.Modeling;

namespace Minecraft2.Bridge;

public static class GraphicChunkData
{
    public static readonly string VertexShader = 
        """
        vec4 process(Input input, out Varyings varyings) {
            varyings.color = input.color;
            varyings.uv = input.uv;
            return camera.proj * camera.view * vec4(input.pos, 1);
        }
        """;

    public static readonly string FragmentShader =
        """
        vec4 process(Varyings varyings, PerPrimitive perPrim) {
            return varyings.color * 
                    perPrim.color * 
                    texture(mainTex, varyings.uv) * 
                    (sin(time.sec)+1)/2.0;
        }
        """;
    
    public struct Varyings()
    {
        [field: GraphicField(ShaderValuePrimitive.Vector3F, "pos")]
        public Vector3 Position;

        [GraphicField(ShaderValuePrimitive.Vector4F, "color")]
        public Vector3 Color;

        [field: GraphicField(ShaderValuePrimitive.Vector2F, "uv")]
        public Vector2 Uv;
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex() : IPositionedVertex, ITexturedVertex
    {
        [field: GraphicField(ShaderValuePrimitive.Vector3F, "pos")]
        public Vector3 Position { get; set; }
        [GraphicField(ShaderValuePrimitive.Vector4F, "color")]
        public Vector3 Color = Vector3.One;
        [field: GraphicField(ShaderValuePrimitive.Vector2F, "uv")]
        public Vector2 Uv { get; set; }
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct Primitive()
    {
        [GraphicField(ShaderValuePrimitive.Vector4F, "color")]
        [FieldOffset(0)] public Vector3 Color = Vector3.One;
    }
}