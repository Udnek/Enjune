using Enjune.File;
using Enjune.Graphic;
using Enjune.Graphic.Api;
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

    public Error? Put(int layer, ByteImage image)
    {
        if (Layers <= layer)
            return $"expected layer <{Layers}, got {layer}";
        
        if (Size != image.Size)
            return $"expected size {Size}, got {image.Size}";
        
        PixelFormat? pixelFormat;
        switch (image.Type.Depth)
        {
            case 4:
                pixelFormat = PixelFormat.Rgba;
                break;
            case 3:
                pixelFormat = PixelFormat.Rgb;
                break;
            case 1:
                image = image.ToRgba32();
                pixelFormat = PixelFormat.Rgba;
                break;
            default:
                pixelFormat = null;
                break;
        }

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