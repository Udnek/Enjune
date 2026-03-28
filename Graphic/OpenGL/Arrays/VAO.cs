using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Arrays;

// ReSharper disable once InconsistentNaming
public sealed class VAO
{
    private readonly int _id;

    public VAO()
    {
        _id = GL.GenVertexArray();
        GL.BindVertexArray(_id);
    }

    public void Destroy()
    {
        GL.DeleteVertexArray(_id);
    }
}