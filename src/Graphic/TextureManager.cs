using Enjune.File;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StbImageSharp;
using Image = SixLabors.ImageSharp.Image;

namespace Enjune.Graphic;

public class TextureManager
{
    private TexId _newId = 0;
    private Dictionary<ResourcePath, TexId> _textureToId = new();
    private bool _compiled = false;

    public readonly TexId ErrorTexture;
    private readonly ResourcePath _errorTexturePath = new("atlas.png");

    public TextureManager()
    {
        ErrorTexture = AddTextureAndGetId(_errorTexturePath);
    }
    
    public TexId AddTextureAndGetId(ResourcePath texturePath)
    {
        if (_compiled)
        {
            Logger.Error(this, "trying to add during compiled state; ignoring");
            return ErrorTexture;
        }
        if (_textureToId.TryGetValue(texturePath, out var id)) 
            return id;
        
        // or else add new
        if (!IsValidTexture(texturePath, out var error))
        {
            Logger.Error(this, $"can not load new texture {texturePath}: {error}");
            return ErrorTexture;
        }
        _textureToId.Add(texturePath, _newId);
        Logger.Log(this, $"added texture {texturePath} with id={_newId}");
        return _newId++;
    }

    private bool IsValidTexture(ResourcePath path, out string? error)
    {
        var imageResult = FileManager.LoadImage(path, out error);
        return imageResult != null;
    }

    public void Compile(out int texSize, out Dictionary<TexId, byte[]> textures)
    {
        if (_compiled) 
            throw new Exception("trying to compile already compiled");
        
        _compiled = true;
        
        // loading
        Dictionary<TexId, ImageResult> rawImages = new();
        foreach (var (path, id) in _textureToId)
        {
            var imageResult = FileManager.LoadImage(path, out var error) ?? throw new Exception(error);
            rawImages[id] = imageResult;
        }

        // choosing max size
        var targetSize = rawImages.MaxBy(img => img.Value.Width).Value.Width;

        // resizing
        Dictionary<TexId, byte[]> resizedImages = new();
        foreach (var (id, rawImage) in rawImages)
        {
            //Logger.Log(this,$"datasize: {rawImage.Data.Length}, wh = {rawImage.Width}, {rawImage.Height}");
            using (var image = Image.LoadPixelData<Rgba32>(rawImage.Data, rawImage.Width, rawImage.Height))
            {
                image.Mutate(c => c.Resize(targetSize, targetSize));
                byte[] buffer = new byte[targetSize * targetSize * 4];
                image.CopyPixelDataTo(buffer);
                resizedImages[id] = buffer;
            }
        }
        texSize = targetSize;
        textures = resizedImages;
    }
}