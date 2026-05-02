using Enjune.Graphic.GraphicApi;
using Enjune.Misc;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Texture;
using OpenGLApi.Component.Uniform;
using OpenGLApi.Data;

namespace OpenGLApi.Shader;

public sealed class MaterialShader : CameraShader, IShader.ICamera.IMaterial
{
    private Vector3Uniform _viewPos = null!;
    private TextureUniform _textureUniform = null!;
    private TextureUniform _shadowMapsUniform = null!;
    
    private readonly int _textureUnitId;
    private readonly int _shadowMapsUnitId;

    public MaterialShader(Fbo fbo, TextureArray textures, TextureArray shadowMaps) : base(fbo)
    {
        _textureUnitId = textures.GetUnitId();
        _shadowMapsUnitId = shadowMaps.GetUnitId();
    }

    protected override void InitUniforms()
    {
        base.InitUniforms();
        _viewPos = new Vector3Uniform("uViewPos", Vector3.Zero, this);
        _textureUniform = new TextureUniform("uTextures", _textureUnitId, this);
        _shadowMapsUniform = new TextureUniform("uShadowMaps", _shadowMapsUnitId, this);
    }

    public void ViewPosition(Position position) => _viewPos.SetValue(position);
}