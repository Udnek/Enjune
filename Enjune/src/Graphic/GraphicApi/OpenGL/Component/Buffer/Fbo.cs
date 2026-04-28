using Enjune.Graphic.GraphicApi.OpenGL.Component.Texture;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Buffer;

public sealed class Fbo : GlDisposable
{
    private readonly int _handle;
    
    public readonly Rbo Rbo;
    public readonly EmptyTexture Texture;

    public Fbo(Vector2i initialSize, TextureUnit unit)
    {
        _handle = GL.GenFramebuffer();
        Texture = new EmptyTexture(unit, initialSize);
        Rbo = new Rbo(initialSize);
        
        Texture.AttachToFbo(this);
        Rbo.AttachToFbo(this);
        
        CheckStatus();
        Unbind();
    }

    public void Resize(Vector2i size)
    {
        Texture.Resize(size);
        Rbo.Resize(size);
        
        Texture.AttachToFbo(this);
        Rbo.AttachToFbo(this);
        
        CheckStatus();
    }

    private void CheckStatus()
    {
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete) 
            Logger.Error(this, $"framebuffer failed: {Enum.GetName(status)}");
    }
    
    public void Bind() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
    public void Unbind() => BindDefault();
    public static void BindDefault() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    
    protected override void DisposeGlData()
    {
        GL.DeleteFramebuffer(_handle);
        Utils.DisposeAllFields(this);
    }
}