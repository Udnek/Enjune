using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Enjune.Graphic.OpenGL.Component.Array;

public class Vao : GLDisposable
{
    private readonly int _handle;

    public Vao()
    {
        _handle = GL.GenVertexArray();
    }
    
    public void Bind() => GL.BindVertexArray(_handle);
    public void Unbind() => GL.BindVertexArray(0);

    protected override void DisposeGLData() => GL.DeleteVertexArray(_handle);
}