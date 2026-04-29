namespace OpenGLApi.Component.Buffer;

public class Vao : GlDisposable
{
    private readonly int _handle = GL.GenVertexArray();

    public void Bind() => GL.BindVertexArray(_handle);
    public void Unbind() => GL.BindVertexArray(0);

    protected override void DisposeGlData() => GL.DeleteVertexArray(_handle);
}