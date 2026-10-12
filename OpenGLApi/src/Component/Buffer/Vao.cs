namespace OpenGLApi.Component.Buffer;

/// <summary>
/// Stores how to interpret VBO
/// </summary>
public class Vao : GlDisposable
{
    private readonly int _handle = GL.GenVertexArray();

    public void Bind() => GL.BindVertexArray(_handle);

    protected override void DisposeGlData() => GL.DeleteVertexArray(_handle);
}