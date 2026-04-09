using Enjune.File;
using Enjune.Graphic.Font;
using Enjune.Misc;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StbImageSharp;
using Image = SixLabors.ImageSharp.Image;

namespace Enjune.Graphic.Asset;

public class AssetManager
{
    private readonly Dictionary<RawMaterial, CompiledMaterial> _materials = [];
    
    private readonly List<ByteImage> _textures = [];
    private readonly HashSet<ResourcePath> _invalidPaths = [];

    public readonly CompiledMaterial MissingMaterial;
    public readonly CompiledMaterial WhiteMaterial;
    
    public AssetManager()
    {
        MissingMaterial = AddMaterialAndGetCompiled(RawMaterial.FromTexture(AssemblyPath.Of(Enjune.Assembly,"MissingTexture.png")));
        WhiteMaterial = AddMaterialAndGetCompiled(RawMaterial.FromTexture(AssemblyPath.Of(Enjune.Assembly,"WhitePixel.png")));
    }

    public CompiledFont? AddFont(ResourcePath path, uint resolution, out string? error)
    {
        FontLoader.Load(out error, resolution, path, out var image, out var rawGlyphs);
        if (image == null || rawGlyphs == null) 
            return null;

        var glyphs = new Dictionary<char, CompiledFont.Glyph>(rawGlyphs.Count);
        foreach (var (ch, rawGlyph) in rawGlyphs)
        {

        }
    }
    
    public CompiledMaterial AddMaterialAndGetCompiled(RawMaterial rawMaterial)
    {
        // same material already exists
        if (_materials.TryGetValue(rawMaterial, out var material)) 
            return material;

        
        var texId = WhiteMaterial.TextureId; // default
        var texturePath = rawMaterial.TexturePath;
        if (rawMaterial.LoadedTexture != null)
        {
            texId = _textures.Count;
            _textures.Add(rawMaterial.LoadedTexture);
        }
        else if (texturePath != null)
        {
            var matWithSameTexture = _materials.FirstOrDefault(
                p => Equals(texturePath, p.Key.TexturePath)).Value;
            // texture already exists
            if (matWithSameTexture != default)
            {
                texId = matWithSameTexture.TextureId;
            }
            // check if it has already been added to invalid
            else if (_invalidPaths.Contains(texturePath))
            {
                return MissingMaterial;
            }
            else // probably should add new
            {
                var loadedTexture = texturePath.LoadImage(out var error);
                // path is invalid
                if (loadedTexture == null)
                {
                    _invalidPaths.Add(texturePath);
                    Logger.Error(this, $"can not load new texture {texturePath}: {error}");
                    return MissingMaterial;
                }
                // adding
                texId = _textures.Count;
                _textures.Add(loadedTexture);
            }
        }
        
        // add new
        MatId matId = _materials.Count;
        var compiledMaterial = new CompiledMaterial(rawMaterial, texId, matId);
        _materials.Add(rawMaterial, compiledMaterial);
        Logger.Log(this, $"added material {compiledMaterial};");
        return compiledMaterial;
    }
    
    public CompiledAssets Compile() // todo add error???
    {
        Logger.Log(this, $"compiling {_textures.Count} textures; {_materials.Count} materials");
        
        var rawImages = _textures;

        // choosing max size
        var targetSize = rawImages.Max(img => img.Width);
        Logger.Log(this, $"target texture size: {targetSize}");

        // resizing
        List<ByteImage> resizedImages = new();
        
        foreach (var rawImage in rawImages)
        {
            ByteImage byteImage = ByteImage.Empty(targetSize, targetSize, rawImage.Type);
            switch (rawImage.Type.Depth)
            {
                case 1:
                {
                    using var image = Image.LoadPixelData<L8>(rawImage.Data, rawImage.Width, rawImage.Height);
                    image.Mutate(c => c.Resize(targetSize, targetSize, KnownResamplers.Box, false));
                    image.CopyPixelDataTo(byteImage.Data);
                    break;
                }
                case 3:
                {
                    using var image = Image.LoadPixelData<Rgb24>(rawImage.Data, rawImage.Width, rawImage.Height);
                    image.Mutate(c => c.Resize(targetSize, targetSize, KnownResamplers.Box, false));
                    image.CopyPixelDataTo(byteImage.Data);
                    break;
                }
                case 4:
                {
                    using var image = Image.LoadPixelData<Rgba32>(rawImage.Data, rawImage.Width, rawImage.Height);
                    image.Mutate(c => c.Resize(targetSize, targetSize, KnownResamplers.Box, false));
                    image.CopyPixelDataTo(byteImage.Data);
                    break;
                }   
                default:
                    Logger.Error(this, $"unsupported texture depth: {rawImage.Type.Depth}");
                    break;
            }
            resizedImages.Add(byteImage);
        }

        Logger.Log(this, $"done compiling");
        return new CompiledAssets(targetSize, resizedImages, 
            _materials
                .OrderBy(e => e.Value.Id)
                .Select(e => e.Value).ToArray());
    }
}