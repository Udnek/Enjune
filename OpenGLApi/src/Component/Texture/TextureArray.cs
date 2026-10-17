using Enjune.File;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using OpenGLApi.Component.Buffer;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenGLApi.Component.Texture;

public sealed class TextureArray : AbstractTexture, ITextureArray
{
    public Vector2i Size { get; }
    public int Layers { get; }

    public TextureArray(Vector2i size, int layers, SizedInternalFormat internalFormat) 
        : base(TextureTarget.Texture2DArray)
    {
        Size = size;
        Layers = layers;
        
        GL.TextureStorage3D(Handle, 1, internalFormat, size.X, size.Y, Layers);
        SetParams();
    }
    
    public static TextureArray FromAssets(CompiledAssets compiledAssets)
    {
        var texArr = new TextureArray((compiledAssets.TextureSize, compiledAssets.TextureSize), 
            compiledAssets.Textures.Count, SizedInternalFormat.Rgba8);

        var textures = compiledAssets.Textures;
        
        // loading into
        for (var layer = 0; layer < textures.Count; layer++)
        {
            texArr.Put(layer, textures[layer]);
            OpenGlApi.CheckGlError(typeof(TextureArray));
        }

        return texArr;
    }

    public Error? Put(int layer, ByteImage image)
    {
        if (Layers <= layer)
            return $"expected layer <{Layers}, got {layer}";
        
        if (Size != image.Size)
            return $"expected size {Size}, got {image.Size}";
        
        PixelFormat? pixelFormat = image.Type.Depth switch
        {
            4 => PixelFormat.Rgba,
            3 => PixelFormat.Rgb,
            1 => PixelFormat.Alpha,
            _ => null
        };
        
        if (pixelFormat == null)
            return $"unsupported depth: {image.Type.Depth}";
        
        GL.TextureSubImage3D(Handle,
            0, // mipmap
            0, 0, layer,
            Size.X, Size.Y, 1,
            (PixelFormat) pixelFormat, PixelType.UnsignedByte, image.Data);
        
        return OpenGlApi.GetGlErr();
    }
}