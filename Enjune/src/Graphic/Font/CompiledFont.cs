using Enjune.Graphic.Asset;
using Enjune.Misc;

namespace Enjune.Graphic.Font;

public class CompiledFont
{
    private readonly Dictionary<char, Glyph> _glyphs;
    private readonly Glyph _fallbackGlyph;
    public readonly CompiledMaterial Material;
    private readonly uint _initialFontHeight;
    
    public record struct Glyph(TextureQuad Texture, uint Height, uint Width, int BearingX, int BearingY, float Advance);

    public CompiledFont(Dictionary<char, Glyph> glyphs, CompiledMaterial material, uint initialFontHeight)
    {
        _glyphs = glyphs;
        _fallbackGlyph = glyphs.GetValueOrDefault('?', glyphs.First().Value);
        Material = material;
        _initialFontHeight = initialFontHeight;
    }

    public void GenerateMeshes(string text, float textHeight, Action<Mesh> consumer)
    {
        if (text.Length == 0)
        {
            Logger.Warn(this, "text is empty");
            return;
        }
        float xOffset = 0;
        float sizeMul = 1f / _initialFontHeight * textHeight;

        // float minY = float.PositiveInfinity;
        // float maxY = float.NegativeInfinity;
        // float minX = float.
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            var glyph = _glyphs.GetValueOrDefault(ch, _fallbackGlyph);
            var width = glyph.Width * sizeMul;
            var height = glyph.Height * sizeMul;
            var mesh = Mesh.Quad(
                (0f, 0f, 0f),
                (width, 0, 0f),
                (width, height, 0f),
                (0, height, 0f),
                glyph.Texture);
            var y = -(glyph.Height - glyph.BearingY) * sizeMul;
            // minY = MathF.Min(y, minY);
            // maxY = MathF.Max(height + y, maxY);
            mesh.Offset(((xOffset + glyph.BearingX) * sizeMul, y, 0f));
            xOffset += glyph.Advance;
            
            consumer(mesh);
        }
    }

    public Model GenerateModel(string text, float height, Color color)
    {
        var meshes = new Mesh[text.Length];
        var i = 0;
        GenerateMeshes(text, height, mesh => meshes[i++] = mesh);

        return new Model(Mesh.Merge(meshes), new Model.PerMesh(Material, color));
    }
}