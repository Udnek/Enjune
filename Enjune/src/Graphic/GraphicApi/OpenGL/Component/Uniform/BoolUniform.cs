namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public sealed class BoolUniform : Uniform<bool>
{
    public BoolUniform(string name, bool initialValue, params ShaderProgram[] programs) : base(name, initialValue, programs)
    {
    }

    protected override void SetValue(int location, bool value) => GL.Uniform1(location, value ? 1 : 0);
}