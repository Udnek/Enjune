namespace Enjune.Graphic.Api.Data;

public enum ShaderValuePrimitive
{
    Int,
    Float,
    Vector2F,
    Vector3F,
    Vector4F,
    Matrix3F,
    Matrix4F
}

public static class ValuePrimitiveExtensions
{
    public static int ByteSize(this ShaderValuePrimitive primitive)
    {
        return primitive switch
        {
            ShaderValuePrimitive.Int => 4,
            ShaderValuePrimitive.Float => 4,
            ShaderValuePrimitive.Vector2F => 8,
            ShaderValuePrimitive.Vector3F => 12,
            ShaderValuePrimitive.Vector4F => 16,
            ShaderValuePrimitive.Matrix3F => 4*3*3,
            ShaderValuePrimitive.Matrix4F => 4*4*4,
            _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null)
        };
    }
}

