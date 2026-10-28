using Enjune.Logger;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace OpenGLApi.Component.Buffer;

/// <summary>
/// Used to connect RBO and Texture, as a render target
/// </summary>
public sealed class Fbo : GlDisposable
{
    public Vector2i Size { get; private set; }
    private readonly int _handle;
    
    public Fbo(Vector2i size)
    {
        Size = size;
        _handle = GL.GenFramebuffer();
    }
    
    public void CheckStatus()
    {
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete) 
            Logger.Error(this, $"Framebuffer failed: {status}");
        OpenGlApi.CheckGlError(this);
    }

    public void Resize(Vector2i size) => Size = size;
    
    public void Bind()
    {
        GL.Viewport(0, 0, Size.X, Size.Y);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
    }

    protected override void DisposeGlData() => GL.DeleteFramebuffer(_handle);
}