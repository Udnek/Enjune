using Enjune.Graphic.Asset;

namespace Enjune.Graphic.Font;

public class CompiledFont
{
    private readonly Dictionary<char, Glyph> _glyphs;
    private readonly Glyph _fallbackGlyph;
    private readonly CompiledMaterial _material;
    private readonly uint _initialFontSize;
    
    public record struct Glyph(TextureQuad Texture, uint Height, uint Width, int BearingX, int BearingY, float Advance);

    public CompiledFont(Dictionary<char, Glyph> glyphs, CompiledMaterial material, uint initialFontSize)
    {
        _glyphs = glyphs;
        _fallbackGlyph = glyphs.GetValueOrDefault('?', glyphs.First().Value);
        _material = material;
        _initialFontSize = initialFontSize;
    }
    
    public Model<(TextureCoord, Vector3),CompiledMaterial> Generate(string text, float size)
    {
        var meshes = new Mesh<(TextureCoord, Vector3)>[text.Length];
        float xOffset = 0;
        float sizeMul = 1f/_initialFontSize *size;
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            var glyph = _glyphs.GetValueOrDefault(ch, _fallbackGlyph);
            var mesh = Mesh<TextureCoord>.Quad(
                (0f,0f, 0f),
                (glyph.Width*sizeMul, 0, 0f),
                (glyph.Width*sizeMul, glyph.Height*sizeMul, 0f),
                (0, glyph.Height*sizeMul, 0f), 
                glyph.Texture);
            mesh.Offset((xOffset + glyph.BearingX * sizeMul,  -(glyph.Height - glyph.BearingY) * sizeMul, 0f));
            xOffset += glyph.Advance * sizeMul;
            meshes[i] = mesh;
        }

        return Model<(TextureCoord, Vector3), CompiledMaterial>.CreateFromOneMaterial(meshes, _material);
    }
}