using OpenTK.Mathematics;

namespace OpenGLApi.Component.Buffer;

public sealed class Rbo : GlDisposable
{
    private int _handle;

    public Rbo(Vector2i size)
    {
        _handle = GL.GenRenderbuffer();
        Bind();
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, size.X, size.Y);
        Unbind();
    }
    
    public void AttachToFbo(Fbo fbo)
    {
        fbo.Bind();
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer, _handle); 
    }

    public void Resize(Vector2i size)
    {
        GL.DeleteRenderbuffer(_handle);
        _handle = GL.GenRenderbuffer();
        Bind();
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, size.X, size.Y);
        Unbind();
    }
    
    private void Bind() => GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _handle);
    private void Unbind() => GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    
    protected override void DisposeGlData() => GL.DeleteRenderbuffer(_handle);
}