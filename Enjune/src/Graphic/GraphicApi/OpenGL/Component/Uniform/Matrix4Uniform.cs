using OpenTK.Mathematics;

namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public sealed class Matrix4Uniform : Uniform<Matrix4>
{
    public Matrix4Uniform(ShaderProgram program, string name) : base(program, name)
    {
        SetValue(Matrix4.Identity);
    }

    public override void SetValue(Matrix4 matrix) => GL.UniformMatrix4(Location, false, ref matrix);
}