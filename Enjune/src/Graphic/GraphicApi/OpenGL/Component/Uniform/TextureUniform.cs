namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;

public sealed class TextureUniform : Uniform<int>
{
    public TextureUniform(ShaderProgram program, string name) : base(program, name) { }
    
    public override void SetValue(int value) => GL.Uniform1(Location, value);
}