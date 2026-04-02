using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Uniform;

public sealed class BoolUniform : Uniform<bool>
{
    public BoolUniform(ShaderProgram program, string name) : base(program, name) { }

    public override void SetValue(bool value) => GL.Uniform1(Location, value ? 1 : 0);
}