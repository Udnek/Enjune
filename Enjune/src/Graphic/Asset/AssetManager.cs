using Enjune.File;
using Enjune.Misc;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StbImageSharp;
using Image = SixLabors.ImageSharp.Image;

namespace Enjune.Graphic.Asset;

public class AssetManager
{
    private readonly Dictionary<RawMaterial, CompiledMaterial> _materials = [];
    private readonly List<ResourcePath> _textures = [];
    private readonly HashSet<ResourcePath> _invalidPaths = [];

    public readonly CompiledMaterial MissingMaterial;
    public readonly CompiledMaterial WhiteMaterial;
    
    public AssetManager()
    {
        MissingMaterial = AddMaterialAndGetCompiled(RawMaterial.FromTexture(AssemblyPath.Of(Enjune.Assembly,"MissingTexture.png")));
        WhiteMaterial = AddMaterialAndGetCompiled(RawMaterial.FromTexture(AssemblyPath.Of(Enjune.Assembly,"WhitePixel.png")));
    }

    public CompiledMaterial AddMaterialAndGetCompiled(RawMaterial rawMaterial)
    {
        // same material already exists
        if (_materials.TryGetValue(rawMaterial, out var material)) 
            return material;

        
        var texId = WhiteMaterial.TextureId; // default
        var texturePath = rawMaterial.TexturePath;
        if (texturePath != null)
        {
            // texture already exists
            if (_textures.Contains(texturePath))
            {
                texId = _textures.IndexOf(texturePath);
            }
            // check if it has already been added to invalid
            else if (_invalidPaths.Contains(texturePath))
            {
                return MissingMaterial;
            }
            else // probably should add new
            {
                // check if texturePath is valid
                if (!IsValidTexture(texturePath, out var error))
                {
                    _invalidPaths.Add(texturePath);
                    Logger.Error(this, $"can not load new texture {texturePath}: {error}");
                    return MissingMaterial;
                }
                
                // adding
                texId = _textures.Count;
                _textures.Add(texturePath);
            }
        }
        
        // add new
        MatId matId = _materials.Count;
        var compiledMaterial = new CompiledMaterial(rawMaterial, texId, matId);
        _materials.Add(rawMaterial, compiledMaterial);
        Logger.Log(this, $"added material {compiledMaterial};");
        return compiledMaterial;
    }

    private bool IsValidTexture(ResourcePath path, out string? error)
    {
        var imageResult = path.LoadImage(out error);
        return imageResult != null;
    }
    
    public CompiledAssets Compile() // todo add error
    {
        Logger.Log(this, $"compiling {_textures.Count} textures; {_materials.Count} materials");
        
        // loading
        List<ImageResult> rawImages = new();
        foreach (var texturePath in _textures)
        {
            var imageResult = texturePath.LoadImage(out var error) 
                              ?? throw new Exception(error);
            rawImages.Add(imageResult);
        }

        // choosing max size
        var targetSize = rawImages.MaxBy(img => img.Width)!.Width;
        Logger.Log(this, $"target texture size: {targetSize}");

        // resizing
        List<byte[]> resizedImages = new();
        foreach (var rawImage in rawImages)
        {
            using (var image = Image.LoadPixelData<Rgba32>(rawImage.Data, rawImage.Width, rawImage.Height))
            {
                image.Mutate(c => c.Resize(targetSize, targetSize, KnownResamplers.Box, false));
                byte[] buffer = new byte[targetSize * targetSize * 4];
                image.CopyPixelDataTo(buffer);
                resizedImages.Add(buffer);
            }
        }

        Logger.Log(this, $"done compiling");
        return new CompiledAssets(targetSize, resizedImages, 
            _materials.OrderBy(e => e.Value.Id)
                .Select(e => e.Value).ToArray());
    }
}