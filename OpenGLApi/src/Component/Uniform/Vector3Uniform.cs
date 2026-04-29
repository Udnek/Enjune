namespace OpenGLApi.Component.Uniform;

public sealed class Vector3Uniform(string name, Vector3 initialValue, ShaderProgram program)
    : Uniform<Vector3>(name, initialValue, program)
{
    protected override void SetValue(int location, Vector3 value) => GL.Uniform3(location, value);
}