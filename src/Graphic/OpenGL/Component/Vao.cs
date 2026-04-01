using OpenTK.Graphics.OpenGL;

namespace Enjune.Graphic.OpenGL.Component;

public class Vao : GLDisposable
{
    private readonly int _handle;

    public Vao()
    {
        _handle = GL.GenVertexArray();
    }

    public void Bind() => GL.BindVertexArray(_handle);

    protected override void DisposeGLData() => GL.DeleteVertexArray(_handle);
}