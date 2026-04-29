namespace OpenGLApi.Component.Uniform;

public sealed class Vector4Uniform(string name, Vector4 initialValue, ShaderProgram program)
    : Uniform<Vector4>(name, initialValue, program)
{
    protected override void SetValue(int location, Vector4 value) => GL.Uniform4(location, value);
}