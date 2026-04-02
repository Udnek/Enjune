using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Uniform;

public sealed class TextureUniform : Uniform<int>
{
    public TextureUniform(ShaderProgram program, string name) : base(program, name) { }
    
    public override void SetValue(int value) => GL.Uniform1(Location, value);
}