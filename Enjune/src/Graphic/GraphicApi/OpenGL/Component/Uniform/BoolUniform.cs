namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public sealed class BoolUniform(string name, bool initialValue, ShaderProgram program)
    : Uniform<bool>(name, initialValue, program)
{
    protected override void SetValue(int location, bool value) => GL.Uniform1(location, value ? 1 : 0);
}