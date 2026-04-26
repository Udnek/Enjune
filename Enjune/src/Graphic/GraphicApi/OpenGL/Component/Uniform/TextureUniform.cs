namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public sealed class TextureUniform(string name, int initialValue, params ShaderProgram[] programs)
    : Uniform<int>(name, initialValue, programs)
{
    protected override void SetValue(int location, int value) => GL.Uniform1(location, value);
}