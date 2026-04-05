using Enjune.Graphic.Asset;
using Enjune.Misc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Texture;

public class TextureArray : GLDisposable
{
    private readonly int _handler;
    private readonly int _size;
    private readonly int _layers;
    
    public TextureArray(CompiledAssets compiledAssets, TextureUnit unit)
    {
        _handler = GL.GenTexture();
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2DArray, _handler);

        var textures = compiledAssets.Textures;
        _size = compiledAssets.TextureSize;
        _layers = textures.Count;

        // allocation
        GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, 
            SizedInternalFormat.Rgba8, _size, _size, _layers);
        
        // params
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        // mipmap generation
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        
        // loading into
        for (var layer = 0; layer < textures.Count; layer++)
        {
            var texture = textures[layer];
            GL.TexSubImage3D(TextureTarget.Texture2DArray,
                0, // mipmap
                0, 0, layer,
                _size, _size, 1,
                PixelFormat.Rgba, PixelType.UnsignedByte, texture);
        }
    }

    public void Dump()
    {
        string path = Path.GetFullPath($"./");
        Logger.Log(this, $"dumping textures into {path}");
        int layerSize = _size * _size * 4;
        byte[] data = new byte[layerSize * _layers];

        GL.GetTexImage(TextureTarget.Texture2DArray, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);

        for (TexId layer = 0; layer < _layers; layer++)
        {
            byte[] layerData = new byte[layerSize];
            System.Array.Copy(data, layer * layerSize, layerData, 0, layerSize);
            
            try
            {
                using (var image = Image.LoadPixelData<Rgba32>(layerData, _size, _size))
                {
                    image.Save(Path.GetFullPath($"{path}/texture_id_{layer}.png"), new PngEncoder());
                }
            }
            catch (Exception e)
            {
                Logger.Error(this, $"can not dump texture {layer}: {e.Message}");
            }
        }
        Logger.Log(this, "done dumping");
    }
    
    protected override void DisposeGLData() => GL.DeleteTexture(_handler);
}