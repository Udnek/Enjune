using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Uniform;

public class BoolUniform
{
    private readonly int _location;

    public BoolUniform(ShaderProgram program, string name)
    {
        _location = program.GetUniformLocation(name);
    }

    public void SetValue(bool value) => GL.Uniform1(_location, value ? 1 : 0);
}