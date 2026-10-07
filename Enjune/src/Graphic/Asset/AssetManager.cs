using System.ComponentModel;
using Enjune.File;
using Enjune.Graphic.Asset.Font;
using Enjune.Misc;
using RectpackSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Enjune.Graphic.Asset;

public class AssetManager
{
    private readonly List<(RawMaterial Raw, CompiledMaterial Compiled)> _materials = [];
    private readonly List<(ByteImage Image, bool ShouldFlip)> _textures = [];
    private readonly HashSet<ResourcePath> _invalidPaths = [];

    public readonly CompiledMaterial MissingMaterial;
    public readonly CompiledMaterial WhiteMaterial;
    
    public AssetManager()
    {
        MissingMaterial = ForceAddAndGetCompiled(AssemblyPath.Of(Enjune.Assembly, "MissingTexture.png"));
        WhiteMaterial = ForceAddAndGetCompiled(AssemblyPath.Of(Enjune.Assembly, "WhitePixel.png"));
    }

    public CompiledFont? AddFont(ResourcePath path, uint height, out Error? error)
    {
        // loading
        FontLoader.Load(out error, height, path, out var rawGlyphs);
        if (rawGlyphs == null) return null;
        
        // packing
        PackingRectangle[] rectangles;
        {
            var rectangleList = new List<PackingRectangle>(rawGlyphs.Count);
            foreach (var (ch, glyph) in rawGlyphs)
            {
                if (glyph.Width == 0 || glyph.Height == 0)
                {
                    Logger.Info(this, $"char '{ch}' ({(byte)ch}) has zero size: {glyph}");
                    continue;
                }
                var rectangle = new PackingRectangle(0, 0, glyph.Width, glyph.Height, id:ch);
                rectangleList.Add(rectangle);
            }
            rectangles = rectangleList.ToArray();
        }
        
        RectanglePacker.Pack(rectangles, out var bounds);
        Logger.Info(this, $"bounds: {bounds.Width}x{bounds.Height}");
        var atlasSize = (int) Math.Pow(2, Math.Ceiling(Math.Log2(Math.Max(bounds.Width, bounds.Height))));
        Logger.Info(this, $"atlas size: {atlasSize}");
        
        // unpacking
        var atlasBuffer = new Buffer2D<byte>(atlasSize, atlasSize);
        foreach (var rectangle in rectangles)
        {
            var rawGlyph = rawGlyphs[(char)rectangle.Id];
            atlasBuffer.PasteFrom(
                new Buffer2D<byte>((int)rawGlyph.Width, (int)rawGlyph.Height, rawGlyph.Buffer),
                (int)rectangle.X, (int)rectangle.Y);
        }
        
        // adding material
        var atlas = new ByteImage(atlasSize, atlasSize, ByteImage.Kind.Alpha8, atlasBuffer.Data);
        atlas = atlas.Alpha8ToRgba32();
        var material = AddMaterialAndGetCompiled(RawMaterial.FromTexture(atlas, path.ToString()));

        // creating compiled font
        var charBounds = rectangles.ToDictionary(rec => (char)rec.Id);
        Dictionary<char, CompiledFont.Glyph> glyphs = new();
        foreach (var (ch, rawGlyph) in rawGlyphs)
        {
            TextureQuad? texture = null;
            if (charBounds.TryGetValue(ch, out var rectangle))
            {
                texture = TextureQuad.FromCorners(
                    (
                        (float) rectangle.X /atlasSize,  
                        (float)(rectangle.Y + rectangle.Height) / atlasSize
                    ),
                    (
                        (float)(rectangle.X + rectangle.Width) / atlasSize, 
                        (float)rectangle.Y / atlasSize
                    ));
            }
          
            glyphs[ch] = new CompiledFont.Glyph
            {
                Texture = texture,
                Height = rawGlyph.Height,
                Width = rawGlyph.Width,
                BearingX = rawGlyph.BearingX,
                BearingY = rawGlyph.BearingY,
                Advance = rawGlyph.Advance
            };
        }

        error = null;
        return new CompiledFont(glyphs, material, height);
    }

    /// <summary>
    /// Only internal use for Missing and White materials
    /// </summary>
    /// <param name="texturePath"></param>
    /// <returns></returns>
    private CompiledMaterial ForceAddAndGetCompiled(ResourcePath texturePath)
    {
        var tex = texturePath.LoadImage(out var error);
        if (tex == null)
        {
            error = $"Can not load image {texturePath}: {error}";
            Logger.Error(this, error);
            throw new Exception(error);
        }
        var matId = _materials.Count;
        var texId = _textures.Count;
        var rawMaterial = RawMaterial.FromTexture(texturePath);
        var compiledMaterial = new CompiledMaterial(rawMaterial, matId, texId);
        _textures.Add((tex, true));
        _materials.Add((rawMaterial, compiledMaterial));
        Logger.Info(this, $"Added material {compiledMaterial}");
        return compiledMaterial;
    }
    
    public CompiledMaterial AddMaterialAndGetCompiled(RawMaterial rawMaterial)
    {
        var texId = WhiteMaterial.TextureId; // default
        var texturePath = rawMaterial.TexturePath;
        if (rawMaterial.LoadedTexture != null)
        {
            var foundTex = _textures.FindIndex(t => Equals(t.Image, rawMaterial.LoadedTexture));
            // texture already present
            if (foundTex != -1) 
                texId = foundTex;
            else
            // adding new
            {
                texId = _textures.Count;
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
                texId = matWithSameTexture.Compiled.TextureId;
            }
            // check if it has already been added to invalid
            else if (_invalidPaths.Contains(texturePath))
            {
                texId = MissingMaterial.TextureId;
            }
            else // probably should add new
            {
                var loadedTexture = texturePath.LoadImage(out var error);
                // path is invalid
                if (loadedTexture == null)
                {
                    _invalidPaths.Add(texturePath);
                    Logger.Error(this, $"Can not load new texture {texturePath}: {error}");
                    return MissingMaterial;
                }
                // adding
                texId = _textures.Count;
                _textures.Add((loadedTexture, true));
            }
        }
        
        // add new
        MatId matId = _materials.Count;
        var compiledMaterial = new CompiledMaterial(rawMaterial, matId, texId);
        _materials.Add((rawMaterial, compiledMaterial));
        Logger.Info(this, $"Added material {compiledMaterial}");
        return compiledMaterial;
    }
    
    public CompiledAssets Compile() // todo add error???
    {
        Logger.Info(this, $"compiling {_textures.Count} textures and {_materials.Count} materials");

        // choosing max size
        var targetSize = _textures.Max(img => img.Image.Width);
        Logger.Info(this, $"target texture size: {targetSize}");

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
                    Logger.Error(this, $"Unsupported texture depth: {rawImage.Type.Depth}");
                    break;
            }
            resizedImages.Add(byteImage);
        }

        Logger.Info(this, "Done compiling");
        return new CompiledAssets(
            WhiteMaterial,
            MissingMaterial,
            targetSize, resizedImages,
            _materials.Select(t => t.Compiled).ToArray());
    }
}