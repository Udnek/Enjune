using Enjune.Graphic.Api;
using Enjune.Logger;
using Enjune.Misc;
using OpenGLApi.Component.Texture;
using OpenGLApi.Pack;

namespace OpenGLApi.Component;

public interface IGlTextureSlot
{
    public SamplerType Type { get; }
    public string InShaderName { get; }
    public int UnitIdx { get; }
}

public enum SamplerType
{
    Sampler2D,
    Sampler2DArray
}

public class GlTextureSlot<TexType> : IGlTextureSlot, ITextureSlot<TexType> where TexType : ITexture
{
    public SamplerType Type { get; }
    public string InShaderName { get; }
    public int UnitIdx { get; }
    private readonly TextureUnit _unit;

    public GlTextureSlot(string inShaderName, SamplerType sampler, int unitIdx)
    {
        Type = sampler;
        InShaderName = inShaderName;
        UnitIdx = unitIdx;
        _unit = TextureUnit.Texture0 + unitIdx;
    }

    public void BindToSample(TexType texture)
    {
        if (texture is AbstractTexture abstractTexture) 
            abstractTexture.BindTo(_unit);
        else if (texture is FboRboTexDevice device)
            device.Texture.BindTo(_unit);
        else
            Logger.Error(this, $"Unsupported texture: {texture}");
    }
}