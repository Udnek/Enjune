namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public class Vector4Uniform : Uniform<Vector4>
{
    public Vector4Uniform(string name, Vector4 initialValue, params ShaderProgram[] programs) : base(name, initialValue, programs)
    {
    }

    protected override void SetValue(int location, Vector4 value) => GL.Uniform4(location, value);
}