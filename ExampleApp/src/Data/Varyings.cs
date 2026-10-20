using Enjune.Graphic.Api.Attribute;
using OpenTK.Mathematics;

namespace ExampleApp.Data;

public struct Varyings
{
    [GraphicField(ShaderValuePrimitive.Vector4F, "color")]
    public Vector4 Color;
    [GraphicField(ShaderValuePrimitive.Vector2F, "uv")]
    public Vector2 Uv;
}