namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public class Vector4Uniform : Uniform<Vector4>
{
    public Vector4Uniform(ShaderProgram program, string name) : base(program, name) { }
    public override void SetValue(Vector4 value) => GL.Uniform4(Location, value);
}