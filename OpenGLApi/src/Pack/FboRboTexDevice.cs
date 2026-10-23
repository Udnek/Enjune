using Enjune.Graphic.Api;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Texture;
using OpenTK.Mathematics;

namespace OpenGLApi.Pack;

public sealed class FboRboTexDevice : GlDisposable, IDynamicTexture
{
    public static Vector2i ScreenSize = new(1, 1);
    
    public readonly Fbo Fbo;
    public readonly Rbo Rbo;
    public readonly ResizableEmptyTexture Texture;

    public FboRboTexDevice(Vector2i initialSize)
    {
        Fbo = new Fbo(initialSize);
        Rbo = new Rbo(initialSize);
        Texture = new ResizableEmptyTexture(initialSize);
        
        AttachEverything();
    }

    public void Resize(Vector2i size)
    {
        if (Size == size) return;
        
        Fbo.Resize(size);
        Texture.Resize(size);
        Rbo.Resize(size);
        
        AttachEverything();
    }

    // binding only fbo, cause texture and rbo need to be bind only during attachment
    public void Bind() => Fbo.Bind();

    public static void BindScreen()
    {
        GL.Viewport(0, 0, ScreenSize.X, ScreenSize.Y);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
    
    private void AttachEverything()
    {
        Fbo.Bind();
        
        // binding texture to fbo
        GL.FramebufferTexture2D(
            FramebufferTarget.Framebuffer, 
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            Texture.Handle, 0);
        OpenGlApi.CheckGlError(this);
        // binding depth to fbo
        GL.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer,
            Rbo.Handle);
        OpenGlApi.CheckGlError(this);
        
        Fbo.CheckStatus();
        
        BindScreen();
    }

    public Vector2i Size
    {
        get => Fbo.Size;
        set => Resize(value);
    }
    
    public void BindAsRenderTarget() => Bind();
    
    protected override void DisposeGlData() => Utils.DisposeAllFields(this);
}