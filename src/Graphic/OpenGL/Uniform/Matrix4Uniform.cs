using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Enjune.Graphic.OpenGL.Uniform;

public class Matrix4Uniform
{
    private readonly int _location;

    public Matrix4Uniform(ShaderProgram program, string name)
    {
        _location = program.GetUniformLocation(name);
        SetValue(Matrix4.Identity);
    }

    public void SetValue(Matrix4 matrix)
    {
        GL.UniformMatrix4(_location, false, ref matrix);
    }
}