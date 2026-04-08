using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Misc;
using FreeTypeSharp;
using RectpackSharp;
using static FreeTypeSharp.FT;

namespace Enjune.Graphic.Font;

public class FontLoader
{
    public record struct RawGlyph(uint Width, uint Height, int BearingX, int BearingY, float Advance, byte[] Buffer);
    
    public record struct CompiledGlyph();
    
    public unsafe string? Load(ResourcePath path)
    {
        FT_LibraryRec_* ft;
        var initError = FT_Init_FreeType(&ft);
        if (initError != FT_Error.FT_Err_Ok) 
            return initError.ToString();

        var fontBytes = path.LoadBytes(out var error);
        if (fontBytes == null) return error;
        
        IntPtr fontMemory = Marshal.AllocHGlobal(fontBytes.Length);
        Marshal.Copy(fontBytes, 0, fontMemory, fontBytes.Length);

        FT_FaceRec_* face;
        var faceError = FT_New_Memory_Face(ft, (byte*)fontMemory, fontBytes.Length, 0, &face);
        if (faceError != FT_Error.FT_Err_Ok) return faceError.ToString();

        FT_Set_Pixel_Sizes(face, 0, 64);
        
        List<(char ch, RawGlyph glyph)> rawGlyphs = new ();
        for (byte charI = 0; charI < 128; charI++)
        {
            var loadError = FT_Load_Char(face, (char)charI, FT_LOAD.FT_LOAD_RENDER);
            if (loadError != FT_Error.FT_Err_Ok) return loadError.ToString();
            
            FT_Bitmap_ bitmap = face->glyph->bitmap;
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
            rawGlyphs.Add(((char) charI, rawGlyph));
        }
        
        // free shit
        Marshal.FreeHGlobal(fontMemory);
        FT_Done_Face(face);
        FT_Done_FreeType(ft);
        
        foreach (var (c, glyph) in rawGlyphs)
        {
            Logger.Log(this, $"{c} ({(byte) c}) = {glyph}");
        }

        var meanWidth = rawGlyphs.Sum(v => v.glyph.Width) / (float) rawGlyphs.Count;
        var meanHeight = rawGlyphs.Sum(v => v.glyph.Height) / (float) rawGlyphs.Count;
        Logger.Log(this, $"meanWidth: {meanWidth}, meanHeight: {meanHeight}");
        Logger.Log(this, $"maxWidth: {rawGlyphs.Max(v => v.glyph.Width)}, " +
                         $"maxHeight: {rawGlyphs.Max(v => v.glyph.Height)}");


        var rectangles = new List<PackingRectangle>(rawGlyphs.Count);
        foreach (var (ch, glyph) in rawGlyphs)
        {
            if (glyph.Width == 0 || glyph.Height == 0)
            {
                Logger.Warn(this, $"char '{ch}' ({(byte) ch}) has zero size: {glyph}");
                continue;
            }
            rectangles.Add(new PackingRectangle(0, 0, glyph.Width, glyph.Height));
        }
        Logger.Log(this, "packing started");
        RectanglePacker.Pack(rectangles.ToArray(), out var bounds);
        Logger.Log(this, $"packing finished: {bounds}");
        return null;
    }
}