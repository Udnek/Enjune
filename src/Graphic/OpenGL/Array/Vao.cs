using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Array;

public class Vao
{
    private readonly int _id;

    public Vao()
    {
        _id = GL.GenVertexArray();
        Bind();
    }

    public void Bind() => GL.BindVertexArray(_id);
    
    public void Destroy() => GL.DeleteVertexArray(_id);
}