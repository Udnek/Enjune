namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public sealed class Vector3Uniform(string name, Vector3 initialValue, params ShaderProgram[] programs)
    : Uniform<Vector3>(name, initialValue, programs)
{
    protected override void SetValue(int location, Vector3 value) => GL.Uniform3(location, value);
}