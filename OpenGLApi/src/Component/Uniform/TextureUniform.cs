namespace OpenGLApi.Component.Uniform;

public sealed class TextureUniform(string name, int initialValue, ShaderProgram program)
    : Uniform<int>(name, initialValue, program)
{
    protected override void SetValue(int location, int value) => GL.Uniform1(location, value);
}