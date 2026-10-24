using Enjune.File;
using Enjune.Graphic;
using Enjune.Misc;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Enjune.KitStart.TAAL;

/// <summary>
/// Used when there are a lot of textures with +- same size
/// </summary>
public class TextureArrayAssetLoader
{
    private readonly List<(RawTAALMaterial Raw, TAALData.Material Material)> _materials = [];
    private readonly List<(ByteImage Image, bool ShouldFlip)> _textures = [];
    private readonly HashSet<ResourcePath> _invalidPaths = [];

    public readonly MatIdx MissingMaterialIndex;
    public readonly TexIdx MissingTextureIndex;
    
    public TextureArrayAssetLoader()
    {
        // loading missing material
        MissingMaterialIndex = ForceAddAndGetIndex(AssemblyPath.Of(Enjune.Assembly, "MissingTexture.png"));
        MissingTextureIndex = MissingMaterialIndex;
    }

    /// <summary>
    /// Only internal use for Missing and White materials
    /// </summary>
    /// <param name="texturePath"></param>
    /// <returns></returns>
    private MatIdx ForceAddAndGetIndex(ResourcePath texturePath)
    {
        var tex = texturePath.LoadImage(out var error);
        if (tex == null)
        {
            error = $"Can not load image {texturePath}: {error}";
            Logger.Error(this, error);
            throw new Exception(error);
        }
        
        var matIdx = _materials.Count;
        var texIdx = _textures.Count;
        var rawMaterial = RawTAALMaterial.FromTexture(texturePath);
        var material = new TAALData.Material
        {
            Color = rawMaterial.Color,
            TextureIndex = texIdx
        };
        _textures.Add((tex, true));
        _materials.Add((rawMaterial, material));
        Logger.Info(this, $"Added material {material} from {texturePath}");
        return matIdx;
    }

    public MatIdx AddMaterialAndGetIndex(RawTAALMaterial rawMaterial)
    {
        var texIdx = MissingMaterialIndex; // fallback
        var texturePath = rawMaterial.TexturePath;
        if (rawMaterial.LoadedTexture != null)
        {
            var foundTex = _textures.FindIndex(t => Equals(t.Image, rawMaterial.LoadedTexture));
            // texture already present
            if (foundTex != -1)
                texIdx = foundTex;
            else 
            {
                // adding new
                texIdx = _textures.Count;
                _textures.Add((rawMaterial.LoadedTexture, false));
            }
        }
        else if (texturePath != null)
        {
            var matWithSameTexture = _materials
                .FirstOrDefault(p => Equals(texturePath, p.Raw.TexturePath));
            // texture already exists
            if (matWithSameTexture != default)
            {
                texIdx = matWithSameTexture.Material.TextureIndex;
            }
            // check if it has already been added to invalid
            else if (_invalidPaths.Contains(texturePath))
            {
                texIdx = MissingTextureIndex;
            }
            else // probably should add new
            {
                var loadedTexture = texturePath.LoadImage(out var error);
                // path is invalid
                if (loadedTexture == null)
                {
                    _invalidPaths.Add(texturePath);
                    Logger.Warn(this, $"Can not load new texture {texturePath}: {error}, using missing");
                    return MissingMaterialIndex;
                }

                // adding new texture
                texIdx = _textures.Count;
                _textures.Add((loadedTexture, true));
            }
        }

        // add new material
        var material = new TAALData.Material()
        {
            Color = rawMaterial.Color,
            TextureIndex = texIdx
        };
        _materials.Add((rawMaterial, material));
        Logger.Info(this, $"Added material {material} from {rawMaterial}");
        return _materials.Count - 1;
    }
    
    public ResultOrError<CompiledTAAL> Compile()
    {
        Logger.Info(this, $"Compiling {_textures.Count} textures and {_materials.Count} materials");

        // choosing max size
        var targetSize = _textures.Max(img => img.Image.Width);
        Logger.Info(this, $"Target texture size: {targetSize}");

        // resizing
        List<ByteImage> resizedImages = new();
        foreach (var (rawImage, shouldFlip) in _textures)
        {
            ByteImage byteImage = ByteImage.Empty(targetSize, targetSize, rawImage.Type);
            switch (rawImage.Type.Depth)
            {
                case 1:
                {
                    using var image = Image.LoadPixelData<L8>(rawImage.Data, rawImage.Width, rawImage.Height);
                    image.Mutate(c =>
                    {
                        if (shouldFlip) c.Flip(FlipMode.Vertical);
                        c.Resize(targetSize, targetSize, KnownResamplers.Box, false);
                    });
                    image.CopyPixelDataTo(byteImage.Data);
                    break;
                }
                case 3:
                {
                    using var image = Image.LoadPixelData<Rgb24>(rawImage.Data, rawImage.Width, rawImage.Height);
                    image.Mutate(c =>
                    {
                        if (shouldFlip) c.Flip(FlipMode.Vertical);
                        c.Resize(targetSize, targetSize, KnownResamplers.Box, false);
                    });
                    image.CopyPixelDataTo(byteImage.Data);
                    break;
                }
                case 4:
                {
                    using var image = Image.LoadPixelData<Rgba32>(rawImage.Data, rawImage.Width, rawImage.Height);
                    image.Mutate(c =>
                    {
                        if (shouldFlip) c.Flip(FlipMode.Vertical);
                        c.Resize(targetSize, targetSize, KnownResamplers.Box, false);
                    });
                    image.CopyPixelDataTo(byteImage.Data);
                    break;
                }   
                default:
                    return new Error($"unsupported texture depth: {rawImage.Type.Depth}");
            }
            resizedImages.Add(byteImage);
        }

        Logger.Info(this, "Done compiling");
        return ResultOrError.Success(
            new CompiledTAAL(
                MissingMaterialIndex, 
                targetSize, 
                resizedImages.ToArray(), 
                _materials.Select(t => t.Material).ToArray())
            );
    }
}