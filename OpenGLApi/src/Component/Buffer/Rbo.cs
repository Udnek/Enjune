using OpenTK.Mathematics;

namespace OpenGLApi.Component.Buffer;

/// <summary>
/// Basically texture that can not be sampled, but I am using it for depth buffer
/// </summary>
public sealed class Rbo : GlDisposable
{
    public int Handle { get; private set; }

    public Rbo(Vector2i size)
    {
        Handle = GL.GenRenderbuffer();
        Bind();
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, 
            RenderbufferStorage.DepthComponent24, size.X, size.Y);
        Unbind();
    }

    public void Resize(Vector2i size)
    {
        GL.DeleteRenderbuffer(Handle);
        Handle = GL.GenRenderbuffer();
        Bind();
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, 
            RenderbufferStorage.DepthComponent24, size.X, size.Y);
        Unbind();
    }

    private void Bind() => GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Handle);
    private void Unbind() => GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    
    protected override void DisposeGlData() => GL.DeleteRenderbuffer(Handle);
}