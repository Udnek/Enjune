using Enjune.File;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Texture;

public sealed class TextureArray : AbstractTexture
{
    private readonly int _size;
    private readonly int _layers;
    
    public TextureArray(CompiledAssets compiledAssets, TextureUnit unit) : base(TextureTarget.Texture2DArray, unit)
    {
        var textures = compiledAssets.Textures;
        _size = compiledAssets.TextureSize;
        _layers = textures.Count;

        // allocation
        GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, 
            SizedInternalFormat.Rgba8, _size, _size, _layers);
        
        // params
        // GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        // GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        // mipmap generation
        GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        
        // loading into
        for (var layer = 0; layer < textures.Count; layer++)
        {
            var texture = textures[layer];
            PixelFormat? pixelFormat = texture.Type.Depth switch
            {
                4 => PixelFormat.Rgba,
                3 => PixelFormat.Rgb,
                1 => PixelFormat.Alpha,
                _ => null
            };
            if (pixelFormat == null)
            {
                Logger.Error(this, $"unsupported depth: {texture.Type.Depth}");
                continue;
            }
            GL.TexSubImage3D(Target,
                0, // mipmap
                0, 0, layer,
                _size, _size, 1,
                (PixelFormat)pixelFormat, PixelType.UnsignedByte, texture.Data);
            
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
        }
    }

    public override Error? Dump(ExternalPath dir, string namePrefix)
    {
        dir = dir.ThisDirectory();
        Logger.Log(this, $"dumping textures into {dir}");
        int layerSize = _size * _size * 4;
        byte[] data = new byte[layerSize * _layers];

        GL.GetTextureImage(Handle, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data.Length, data);

        for (TexId layer = 0; layer < _layers; layer++)
        {
            byte[] layerData = new byte[layerSize];
            System.Array.Copy(data, layer * layerSize, layerData, 0, layerSize);
            
            try
            {
                using var image = Image.LoadPixelData<Rgba32>(layerData, _size, _size);
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