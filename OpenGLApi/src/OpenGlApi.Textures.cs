using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Texture;
using OpenGLApi.Pack;
using OpenTK.Mathematics;

namespace OpenGLApi;

public sealed partial class OpenGlApi
{
    public ResultOrError<ITextureSlot<TexType>> CreateTextureSlot<TexType>(string inShaderName) where TexType : ITexture
    {
        SamplerType samplerType;
        if (typeof(TexType) == typeof(ITexture))
            samplerType = SamplerType.Sampler2D;
        else if (typeof(TexType) == typeof(ITextureArray))
            samplerType = SamplerType.Sampler2DArray;
        else if (typeof(TexType) == typeof(IDynamicTexture))
            samplerType = SamplerType.Sampler2D;
        else
            return new Error($"unknown type: {Logger.GetTypeName<TexType>()}");

        var slot = new GlTextureSlot<TexType>(inShaderName, samplerType, StaticBindings.TakeTexUnit(inShaderName));
        _textureSlots.Add(slot);
        return ResultOrError.Success<ITextureSlot<TexType>>(slot);
    }

    public ResultOrError<ITexture> CreateTexture(ByteImage image)
    {
        var tex = new Texture(image.Size);
        _texturesToDispose.Add(tex);
        
        var error = tex.Put(image);
        if (error != null) return error.Value;
        return ResultOrError.Success<ITexture>(tex);
    }
    
    public ResultOrError<ITextureArray> CreateTextureArray(ReadOnlySpan<ByteImage> images)
    {
        if (images.IsEmpty)
            return new Error("image array can not be empty");

        var size = images[0].Size;
        foreach (var image in images)
        {
            if (image.Size != size)
                return new Error($"sizes are different: {size}, {image.Size}");
        }
        
        var texArr = new TextureArray(size, images.Length, SizedInternalFormat.Rgb8);
        _texturesToDispose.Add(texArr);
        
        // loading into
        for (var layer = 0; layer < images.Length; layer++)
        {
            var putErr = texArr.Put(layer, images[layer]);
            if (putErr != null)
                return new Error($"Can not put layer {layer}: {putErr}");
        }

        return ResultOrError.Success<ITextureArray>(texArr);
    }

    public ResultOrError<IDynamicTexture> CreateDynamicTexture(Vector2i size)
    {
        if (size.X <= 0 || size.Y <= 0)
            return new Error($"size must be positive, got {size}");
        
        var device = new FboRboTexDevice(size);
        _rboFboToDispose.Add(device);
        return ResultOrError.Success<IDynamicTexture>(device);
    }
}