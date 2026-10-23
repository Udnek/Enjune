using Enjune.File;
using Enjune.Misc;
using OpenGLApi.Component.Buffer;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenGLApi.Component.Texture;

public sealed class ResizableEmptyTexture : AbstractTexture
{
    private Vector2i _size;
    
    public ResizableEmptyTexture(Vector2i size) : base(TextureTarget.Texture2D)
    {
        _size = size;
        Setup();
    }
    
    public void Resize(Vector2i size)
    {
        _size = size;
        GL.DeleteTexture(Handle);
        GenTexture();
        Setup();
    }

    private void Setup()
    {
        GL.TextureStorage2D(Handle, 1, SizedInternalFormat.Rgb8, _size.X, _size.Y);
        SetParams();
    }
}