using Enjune.Graphic.GraphicApi;
using Enjune.Misc;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Texture;
using OpenGLApi.Component.Uniform;
using OpenGLApi.Data;

namespace OpenGLApi.Shader;

public sealed class MaterialShader : Shader3D, IShader.I3D.IMaterial
{
    private Vector3Uniform _viewPos = null!;
    private IntUniform _lightsLength = null!;
    private TextureUniform _textureUniform = null!;
    
    private readonly int _textureUnitId;
    private readonly Ssbo<PointLightData> _lightSsbo;

    public MaterialShader(TextureArray textures, Ssbo<PointLightData> lightSsbo)
    {
        _textureUnitId = textures.GetUnitId();
        _lightSsbo = lightSsbo;
    }

    protected override void InitUniforms()
    {
        base.InitUniforms();
        _viewPos = new Vector3Uniform("uViewPos", Vector3.Zero, this);
        _textureUniform = new TextureUniform("uTextures", _textureUnitId, this);
        _lightsLength = new IntUniform("uLightsLength", 0, this);
    }


    public void Lights(IEnumerable<PointLight> lights)
    {
        int count = lights.Count();
        if (count > _lightSsbo.Capacity)
        {
            Logger.Warn(this,$"lights size to big: {count}, but max capacity is {_lightSsbo.Capacity}");
            count = _lightSsbo.Capacity;
        }

        _lightSsbo.BindAndPush(lights.Select(l => new PointLightData(l.Position, l.Color)).ToArray());
        _lightsLength.SetValue(count);
    }

    public void ViewPosition(Position position) => _viewPos.SetValue(position);
}