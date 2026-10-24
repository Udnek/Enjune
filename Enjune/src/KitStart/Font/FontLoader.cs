using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Graphic;
using Enjune.KitStart.TAAL;
using Enjune.Misc;
using FreeTypeSharp;
using RectpackSharp;
using static FreeTypeSharp.FT;

namespace Enjune.KitStart.Font;

public static class FontLoader
{
    private record struct RawGlyph(uint Width, uint Height, int BearingX, int BearingY, float Advance, byte[] Buffer);
    
    public static ResultOrError<(CompiledFont Font, ByteImage GlyphAtlas)> LoadFont(ResourcePath path, uint height)
    {
        // loading
        LoadGlyphs(out var error, height, path, out var rawGlyphs);
        if (rawGlyphs is null) return error!.Value;
        
        // packing
        PackingRectangle[] rectangles;
        {
            var rectangleList = new List<PackingRectangle>(rawGlyphs.Count);
            foreach (var (ch, glyph) in rawGlyphs)
            {
                if (glyph.Width == 0 || glyph.Height == 0)
                {
                    Logger.Info(typeof(FontLoader), $"char '{ch}' ({(byte)ch}) has zero size: {glyph}");
                    continue;
                }
                var rectangle = new PackingRectangle(0, 0, glyph.Width, glyph.Height, id:ch);
                rectangleList.Add(rectangle);
            }
            rectangles = rectangleList.ToArray();
        }
        
        RectanglePacker.Pack(rectangles, out var bounds);
        Logger.Info(typeof(FontLoader), $"Bounds: {bounds.Width}x{bounds.Height}");
        var atlasSize = (int) Math.Pow(2, Math.Ceiling(Math.Log2(Math.Max(bounds.Width, bounds.Height))));
        Logger.Info(typeof(FontLoader), $"Atlas size: {atlasSize}");
        
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
        var atlas = new ByteImage(atlasSize, atlasSize, ByteImage.Format.Grey8, atlasBuffer.Data);

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

        return ResultOrError.Success((new CompiledFont(glyphs, height), atlas));
    }
    
    private static unsafe void LoadGlyphs(out Error? error, uint pixelHeight, ResourcePath path, out Dictionary<char, RawGlyph>? rawGlyphsOut)
    {
        rawGlyphsOut = null;
        FT_LibraryRec_* ft;
        var initError = FT_Init_FreeType(&ft);
        if (initError != FT_Error.FT_Err_Ok)
        {
            error = initError.ToString();
            return;
        }

        var fontBytes = path.LoadBytes(out var loadBytesError);
        if (fontBytes == null)
        {
            error = loadBytesError;
            FT_Done_FreeType(ft);
            return;
        }
        
        IntPtr fontMemory = Marshal.AllocHGlobal(fontBytes.Length);
        Marshal.Copy(fontBytes, 0, fontMemory, fontBytes.Length);
        
        FT_FaceRec_* face;
        var faceError = FT_New_Memory_Face(ft, (byte*)fontMemory, fontBytes.Length, 0, &face);
        if (faceError != FT_Error.FT_Err_Ok)
        {
            error = faceError.ToString();
            Marshal.FreeHGlobal(fontMemory);
            FT_Done_FreeType(ft);
            return;
        }

        FT_Set_Pixel_Sizes(face, 0, pixelHeight);
        
        var rawGlyphs = new Dictionary<char, RawGlyph>();
        for (byte charI = 0; charI < 128; charI++)
        {
            var ch = (char) charI;
            var loadError = FT_Load_Char(face, ch, FT_LOAD.FT_LOAD_RENDER);
            if (loadError != FT_Error.FT_Err_Ok)
            {
                error = loadError.ToString();
                Marshal.FreeHGlobal(fontMemory);
                FT_Done_Face(face);
                FT_Done_FreeType(ft);
                return;
            }
            
            var bitmap = face->glyph->bitmap;
            
            // copying to local buffer
            byte[] buffer = new byte[bitmap.width * bitmap.rows];
            for (int i = 0; i < bitmap.width * bitmap.rows; i++)
                buffer[i] = bitmap.buffer[i];
            
            var rawGlyph = new RawGlyph(
                Width: bitmap.width,
                Height: bitmap.rows,
                BearingX: face->glyph->bitmap_left,
                BearingY: face->glyph->bitmap_top,
                Advance: (int)face->glyph->advance.x/64f,
                Buffer: buffer
                );
            rawGlyphs[ch] = rawGlyph;
        }
        
        Marshal.FreeHGlobal(fontMemory);
        FT_Done_Face(face);
        FT_Done_FreeType(ft);
        
        error = null;
        rawGlyphsOut = rawGlyphs;
    }
}











