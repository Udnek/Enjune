using Enjune.Misc;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Texture;

namespace OpenGLApi.Pack;

public sealed class ShadowMapPack : GlDisposable
{
    public readonly Fbo Fbo;
    public readonly TextureArray Maps;
    
    public ShadowMapPack(int shadowResolution, TextureUnit textureUnit, int maxLights)
    {
        Fbo = new Fbo((shadowResolution, shadowResolution));
        Maps = TextureArray.Empty(textureUnit, (shadowResolution, shadowResolution), maxLights,
            SizedInternalFormat.DepthComponent32f, PixelFormat.DepthComponent);
        Fbo.SetEmptyColorBuffer();
    }

    public void BindFbo(int lightId) => Maps.AttachToFbo(Fbo, FramebufferAttachment.DepthAttachment, lightId);

    protected override void DisposeGlData() => Utils.DisposeAllFields(this);
}