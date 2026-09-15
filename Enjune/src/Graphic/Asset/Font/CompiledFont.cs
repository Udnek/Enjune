using System.Diagnostics.Contracts;
using Enjune.Graphic.Modeling;

namespace Enjune.Graphic.Asset.Font;

public class CompiledFont
{
    public readonly CompiledMaterial Material;
    
    private readonly Dictionary<char, Glyph> _glyphs;
    private readonly Glyph _fallbackGlyph;
    private readonly uint _initialFontHeight;
    
    /// <summary>
    /// https://freetype.sourceforge.net/freetype2/docs/glyphs/Image3.png
    /// </summary>
    /// <param name="Texture"></param>
    /// <param name="Height"></param>
    /// <param name="Width"></param>
    /// <param name="BearingX"></param>
    /// <param name="BearingY"></param>
    /// <param name="Advance"></param>
    public record struct Glyph(TextureQuad? Texture, uint Height, uint Width, int BearingX, int BearingY, float Advance);

    public CompiledFont(Dictionary<char, Glyph> glyphs, CompiledMaterial material, uint initialFontHeight)
    {
        _glyphs = glyphs;
        _fallbackGlyph = glyphs.GetValueOrDefault('?', glyphs.First().Value);
        Material = material;
        _initialFontHeight = initialFontHeight;
    }

    /// <summary>
    /// Estimates the total size of text as one line (not counting \n)
    /// </summary>
    /// <param name="line"></param>
    /// <param name="textHeight"></param>
    /// <returns></returns>
    [Pure]
    public (float width, float minY, float maxY) EstimateLineSize(string line, float textHeight)
    {
        float width = 0;
        float maxY = 0;
        float minY = 0;
        foreach (var ch in line)
        {
            var glyph = _glyphs.GetValueOrDefault(ch, _fallbackGlyph);
            width += glyph.Advance;
            maxY = Math.Max(maxY, glyph.BearingY);
            minY = Math.Min(minY, glyph.BearingY - glyph.Height);
        }
        
        // we are calculating it cause font is already rendered in some size
        var sizeMul = textHeight / _initialFontHeight;

        return (width*sizeMul, minY*sizeMul, maxY*sizeMul);
    }

    [Pure]
    public (float width, float minY, float maxY) EstimateTextSize(string[] lines, float textHeight, float lineSpacing)
    {
        if (lines.Length == 0) 
            return (0, 0, 0);
        if (lines.Length == 1)
            return EstimateLineSize(lines[0], textHeight);

        float maxY = 0;
        float minY = 0;
        float width = 0;
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var (lineWidth, lineMinY, lineMaxY) = EstimateLineSize(line, textHeight);
            width = Math.Max(width, lineWidth);
            maxY = Math.Max(maxY, lineMaxY - lineSpacing * i);
            minY = Math.Min(minY, lineMinY - lineSpacing * i);
        }

        return (width, minY, maxY);
    }
    
    public void GenerateMeshes(string text, float textHeight, Action<(Mesh Mesh, int CharIdx)> consumer)
    {
        // we are calculating it cause font is already rendered in some size
        var sizeMul = textHeight / _initialFontHeight;
     
        var xOffset = 0f;
        for (var index = 0; index < text.Length; index++)
        {
            var ch = text[index];
            var glyph = _glyphs.GetValueOrDefault(ch, _fallbackGlyph);

            if (glyph.Texture is not null)
            {
                var width = glyph.Width * sizeMul;
                var height = glyph.Height * sizeMul;
                var mesh = Mesh.Quad(
                    (0f, 0f, 0f),
                    (width, 0, 0f),
                    (width, height, 0f),
                    (0, height, 0f),
                    glyph.Texture.Value);

                var y = -(glyph.Height - glyph.BearingY) * sizeMul;
                mesh.Offset(((xOffset + glyph.BearingX) * sizeMul, y, 0f));
                consumer((mesh, index));
            }

            // if texture is null we only advance
            xOffset += glyph.Advance;
        }
    }

    public Model GenerateModel(string text, float height, Color color)
    {
        var meshes = new Mesh[text.Length];
        var i = 0;
        GenerateMeshes(text, height, mesh => meshes[i++] = mesh.Mesh);

        return new Model(Mesh.Merge(meshes), new Model.PerMesh(Material, color));
    }
}