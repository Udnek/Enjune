namespace Enjune.Graphic.Api.Data;

public enum ValuePrimitive
{
    Int,
    Float,
    Vector3F,
    Vector4F,
    Matrix3F,
    Matrix4F
}

public static class ValuePrimitiveExtensions
{
    public static int ByteSize(this ValuePrimitive primitive)
    {
        return primitive switch
        {
            ValuePrimitive.Int => 4,
            ValuePrimitive.Float => 4,
            ValuePrimitive.Vector3F => 4*3,
            ValuePrimitive.Vector4F => 4*4,
            ValuePrimitive.Matrix3F => 4*3*3,
            ValuePrimitive.Matrix4F => 4*4*4,
            _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null)
        };
    }
}

