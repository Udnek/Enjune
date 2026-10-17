using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace OpenGLApi.Component.Texture;

public class Texture : AbstractTexture, ITexture
{
    public Vector2i Size { get; }
    
    public Texture(Vector2i size) : base(TextureTarget.Texture2D)
    {
        Size = size;
        GL.TextureStorage2D(Handle, 1, SizedInternalFormat.Rgba8, size.X, size.Y);
        SetParams();
    }
    
    public Error? Put(ByteImage image)
    {
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
        
        GL.TextureSubImage2D(Handle,
            0, // mipmap
            0, 0,
            image.Height, image.Width,
            (PixelFormat) pixelFormat, PixelType.UnsignedByte, image.Data);
        
        return OpenGlApi.GetGlErr();
    }
}