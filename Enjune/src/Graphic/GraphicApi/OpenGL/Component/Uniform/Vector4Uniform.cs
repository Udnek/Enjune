namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public sealed class Vector4Uniform(string name, Vector4 initialValue, params ShaderProgram[] programs)
    : Uniform<Vector4>(name, initialValue, programs)
{
    protected override void SetValue(int location, Vector4 value) => GL.Uniform4(location, value);
}