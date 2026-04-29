using Enjune.File;
using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using OpenGLApi.Component.Buffer;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenGLApi.Component.Texture;

public sealed class TextureArray : AbstractTexture
{
    private readonly Vector2i _size;
    private readonly int _layers;
    private readonly SizedInternalFormat _internalFormat;
    private readonly PixelFormat _pixelFormat;

    private TextureArray(TextureUnit unit, Vector2i size, int layers, SizedInternalFormat internalFormat, PixelFormat pixelFormat) : base(TextureTarget.Texture2DArray, unit)
    {
        _size = size;
        _layers = layers;
        _internalFormat = internalFormat;
        _pixelFormat = pixelFormat;

        GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, _internalFormat, size.X, size.Y, _layers);
        GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }
    
    public void AttachToFbo(Fbo fbo, FramebufferAttachment attachmentType, int layer)
    {
        fbo.Bind();
        GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, attachmentType, Handle, 0, layer);
    }

    public static TextureArray Empty(TextureUnit unit, Vector2i size, int layers, SizedInternalFormat internalFormat, PixelFormat pixelFormat) 
        => new(unit, size, layers, internalFormat, pixelFormat);
    
    public static TextureArray FromAssets(TextureUnit unit, CompiledAssets compiledAssets)
    {
        var texArr = new TextureArray(unit, (compiledAssets.TextureSize, compiledAssets.TextureSize), 
            compiledAssets.Textures.Count, SizedInternalFormat.Rgba8, PixelFormat.Rgba);

        var textures = compiledAssets.Textures;
        
        // loading into
        for (var layer = 0; layer < textures.Count; layer++)
        {
            texArr.Put(layer, textures[layer]);
            OpenGlApi.CheckGlError();
        }


        GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);

        return texArr;
    }

    public void Put(int layer, ByteImage image)
    {
        PixelFormat? pixelFormat = image.Type.Depth switch
        {
            4 => PixelFormat.Rgba,
            3 => PixelFormat.Rgb,
            1 => PixelFormat.Alpha,
            _ => null
        };
        if (pixelFormat == null)
        {
            Logger.Error(this,$"unsupported depth: {image.Type.Depth}");
            return;
        }
        
        GL.TexSubImage3D(Target,
            0, // mipmap
            0, 0, layer,
            _size.X, _size.Y, 1,
            (PixelFormat) pixelFormat, PixelType.UnsignedByte, image.Data);
    }

    public override Error? Dump(ExternalPath dir, string namePrefix)
    {
        dir = dir.ThisDirectory();
        Logger.Log(this, $"dumping textures into {dir}");
        int layerSize = _size.X * _size.Y * 4;
        byte[] data = new byte[layerSize * _layers];

        GL.GetTextureImage(Handle, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data.Length, data);

        for (TexId layer = 0; layer < _layers; layer++)
        {
            byte[] layerData = new byte[layerSize];
            Array.Copy(data, layer * layerSize, layerData, 0, layerSize);
            
            try
            {
                using var image = Image.LoadPixelData<Rgba32>(layerData, _size.X, _size.Y);
                image.Save(dir.ResolveRaw($"{namePrefix}_layer_{layer}.png").ToString(), new PngEncoder());
            }
            catch (Exception e)
            {
                return $"can not dump texture {layer}: {e.Message}";
            }
        }
        Logger.Log(this, "done dumping");
        return null;
    }
}