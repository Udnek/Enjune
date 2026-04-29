using Enjune.Misc;

namespace OpenGLApi.Component.Buffer;

public sealed class Fbo : GlDisposable
{
    private readonly int _handle;
    
    public Fbo()
    {
        _handle = GL.GenFramebuffer();
    }
    
    public void CheckStatus()
    {
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete) 
            Logger.Error(this, $"framebuffer failed: {Enum.GetName(status)}");
    }

    public void SetEmptyColorBuffer()
    {
        Bind();
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
    }
    
    public void Bind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
    public void Unbind() => BindDefault();
    
    public static void BindDefault() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    
    protected override void DisposeGlData() => GL.DeleteFramebuffer(_handle);
}