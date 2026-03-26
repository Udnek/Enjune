using OpenTK.Graphics.OpenGL4;

namespace Engine.Graphic.OpenGL.Arrays;

public class VAO
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