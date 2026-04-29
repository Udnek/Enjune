using Enjune.Misc;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Texture;
using OpenTK.Mathematics;

namespace OpenGLApi.Component;

public sealed class ScreenPack : GlDisposable
{
    public readonly Fbo Fbo;
    public readonly Rbo Rbo;
    public readonly EmptyTexture Texture;

    public ScreenPack(Vector2i initialSize, TextureUnit textureUnit)
    {
        Fbo = new Fbo();
        Rbo = new Rbo(initialSize);
        Texture = new EmptyTexture(textureUnit, initialSize);
        
        Texture.AttachToFbo(Fbo, FramebufferAttachment.ColorAttachment0);
        Rbo.AttachToFbo(Fbo);
        
        Fbo.CheckStatus();
    }

    public void BindFbo() => Fbo.Bind();

    public void Resize(Vector2i size)
    {
        Texture.Resize(size);
        Rbo.Resize(size);
        
        Texture.AttachToFbo(Fbo, FramebufferAttachment.ColorAttachment0);
        Rbo.AttachToFbo(Fbo);
        
        Fbo.CheckStatus();
    }
    
    protected override void DisposeGlData() => Utils.DisposeAllFields(this);
}