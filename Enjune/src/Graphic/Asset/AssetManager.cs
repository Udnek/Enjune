using Enjune.File;
using Enjune.Graphic.Font;
using Enjune.Misc;
using RectpackSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StbImageSharp;
using Image = SixLabors.ImageSharp.Image;

namespace Enjune.Graphic.Asset;

public class AssetManager
{
    private readonly Dictionary<RawMaterial, CompiledMaterial> _materials = [];
    
    private readonly List<(ByteImage image, bool shouldFlip)> _textures = [];
    private readonly HashSet<ResourcePath> _invalidPaths = [];

    public readonly CompiledMaterial MissingMaterial;
    public readonly CompiledMaterial WhiteMaterial;
    
    
    public AssetManager()
    {
        MissingMaterial = AddMaterialAndGetCompiled(RawMaterial.FromTexture(AssemblyPath.Of(Enjune.Assembly,"MissingTexture.png")));
        WhiteMaterial = AddMaterialAndGetCompiled(RawMaterial.FromTexture(AssemblyPath.Of(Enjune.Assembly,"WhitePixel.png")));
    }

    public CompiledFont? AddFont(ResourcePath path, uint resolution, out Error? error)
    {
        FontLoader.Load(out error, resolution, path, out var rawGlyphs);
        if (rawGlyphs == null) return null;
        
        // packing
        PackingRectangle[] rectangles;
        {
            var rectangleList = new List<PackingRectangle>(rawGlyphs.Count);
            foreach (var (ch, glyph) in rawGlyphs)
            {
                if (glyph.Width == 0 || glyph.Height == 0)
                {
                    Logger.Log(typeof(FontLoader), $"char '{ch}' ({(byte)ch}) has zero size: {glyph}");
                    continue;
                }
                var rectangle = new PackingRectangle(0, 0, glyph.Width, glyph.Height, id:ch);
                rectangleList.Add(rectangle);
            }
            rectangles = rectangleList.ToArray();
        }

        
        RectanglePacker.Pack(rectangles, out var bounds);
        Logger.Log(typeof(FontLoader), $"bounds: {bounds.Width}x{bounds.Height}");
        var atlasSize = (int) Math.Pow(2, Math.Ceiling(Math.Log2(Math.Max(bounds.Width, bounds.Height))));
        Logger.Log(typeof(FontLoader), $"atlas size: {atlasSize}");

        var atlasBuffer = new Buffer2D<byte>(atlasSize, atlasSize);
        foreach (var rectangle in rectangles)
        {
            var rawGlyph = rawGlyphs[(char)rectangle.Id];
            atlasBuffer.PasteFrom(
                new Buffer2D<byte>((int)rawGlyph.Width, (int)rawGlyph.Height, rawGlyph.Buffer),
                (int)rectangle.X, (int)rectangle.Y);
        }
        
        var atlas = new ByteImage(atlasSize, atlasSize, ByteImage.ImType.Alpha8, atlasBuffer.Data);
        atlas = atlas.Alpha8ToRgba32();
        var material = AddMaterialAndGetCompiled(RawMaterial.FromTexture(atlas, path.ToString()));


        Dictionary<char, PackingRectangle> charBounds = rectangles.ToDictionary(rec => (char)rec.Id);
        Dictionary<char, CompiledFont.Glyph> glyphs = new();
        foreach (var (ch, rawGlyph) in rawGlyphs)
        {
            var rectangle = charBounds.GetValueOrDefault(ch, new PackingRectangle(0, 0, 0, 0));
            var texture = new TextureQuad(
                (
                    (float) rectangle.X /atlasSize,  
                    (float)(rectangle.Y + rectangle.Height) / atlasSize
                    ),
                (
                    (float)(rectangle.X + rectangle.Width) / atlasSize, 
                    (float)rectangle.Y / atlasSize
                    ));
          
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
        return new CompiledFont(glyphs, material, resolution);
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
            var foundTex = _textures.FindIndex(t => Equals(t.image, rawMaterial.LoadedTexture));
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
                texId = MissingMaterial.TextureId;
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
                _textures.Add((loadedTexture, true));
            }
        }
        
        // add new
        MatId matId = _materials.Count;
        var compiledMaterial = new CompiledMaterial(rawMaterial, matId, texId);
        _materials.Add(rawMaterial, compiledMaterial);
        Logger.Log(this, $"added material {compiledMaterial};");
        return compiledMaterial;
    }
    
    public CompiledAssets Compile() // todo add error???
    {
        Logger.Log(this, $"compiling {_textures.Count} textures; {_materials.Count} materials");

        // choosing max size
        var targetSize = _textures.Max(img => img.image.Width);
        Logger.Log(this, $"target texture size: {targetSize}");

        // resizing
        List<ByteImage> resizedImages = new();
        foreach (var rawImageTuple in _textures)
        {
            var rawImage = rawImageTuple.image;
            ByteImage byteImage = ByteImage.Empty(targetSize, targetSize, rawImage.Type);
            switch (rawImage.Type.Depth)
            {
                case 1:
                {
                    using var image = Image.LoadPixelData<L8>(rawImage.Data, rawImage.Width, rawImage.Height);
                    image.Mutate(c =>
                    {
                        if (rawImageTuple.shouldFlip) c.Flip(FlipMode.Vertical);
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
                        if (rawImageTuple.shouldFlip) c.Flip(FlipMode.Vertical);
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
                        if (rawImageTuple.shouldFlip) c.Flip(FlipMode.Vertical);
                        c.Resize(targetSize, targetSize, KnownResamplers.Box, false);
                    });
                    image.CopyPixelDataTo(byteImage.Data);
                    break;
                }   
                default:
                    Logger.Error(this, $"unsupported texture depth: {rawImage.Type.Depth}");
                    break;
            }
            resizedImages.Add(byteImage);
        }

        Logger.Log(this, "done compiling");
        return new CompiledAssets(targetSize, resizedImages, 
            _materials
                .OrderBy(e => e.Value.Id)
                .Select(e => e.Value).ToArray());
    }
}