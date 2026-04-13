using OpenTK.Mathematics;

namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public sealed class Matrix4Uniform : Uniform<Matrix4>
{
    public Matrix4Uniform(string name, Matrix4 initialValue, params ShaderProgram[] programs) : base(name, initialValue, programs)
    {
    }

    protected override void SetValue(int location, Matrix4 value)
    {
        GL.UniformMatrix4(location, false, ref value);
    }
}