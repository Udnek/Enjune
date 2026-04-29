namespace OpenGLApi.Component.Uniform;

public sealed class Matrix4Uniform(string name, Matrix4 initialValue, ShaderProgram program)
    : Uniform<Matrix4>(name, initialValue, program)
{
    protected override void SetValue(int location, Matrix4 value)
    {
        GL.UniformMatrix4(location, false, ref value);
    }
}