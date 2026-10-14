using Enjune.Graphic.Api.Data;
using OpenTK.Mathematics;

namespace ExampleApp;

public struct Varyings
{
    [GraphicField(ShaderValuePrimitive.Vector4F, "color")]
    public Vector4 Color;
}