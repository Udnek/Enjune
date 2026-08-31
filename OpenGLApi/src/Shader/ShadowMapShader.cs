using Enjune.Graphic.Api;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Uniform;
using OpenGLApi.Data;
using OpenGLApi.Pack;

namespace OpenGLApi.Shader;

public class ShadowMapShader : AbstractShader, IShader.IShadowMap
{
    private readonly SsboDataAndArray<LightsLengthData, SpotLightData> _lightSsbo;
    private readonly ShadowMapPack _shadowMapPack;
    private Matrix4Uniform _model = null!;
    private IntUniform _lightId = null!;

    public ShadowMapShader(SsboDataAndArray<LightsLengthData, SpotLightData> lightSsbo, ShadowMapPack shadowMapPack)
    {
        _lightSsbo = lightSsbo;
        _shadowMapPack = shadowMapPack;
    }
    
    protected override void InitUniforms()
    {
        _model = new Matrix4Uniform("uModel", Matrix4.Identity, this);
        _lightId = new IntUniform("uLightId", 0, this);
    }

    public override void AfterBind() {}

    public void ForEachLight(Action action)
    {
        for (int lightId = 0; lightId < _lightSsbo.CurrentData.LightsLength; lightId++)
        {
            _lightId.SetValue(lightId);
            _shadowMapPack.BindFbo(lightId);
            action();
        }
    }

    public void ModelTransform(Matrix4 model) => _model.SetValue(model);
}