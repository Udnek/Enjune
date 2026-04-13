using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using FreeTypeSharp;
using RectpackSharp;
using static FreeTypeSharp.FT;

namespace Enjune.Graphic.Font;

public static class FontLoader
{
    public record struct RawGlyph(uint Width, uint Height, int BearingX, int BearingY, float Advance, byte[] Buffer);
    
    public static unsafe void Load(out Error? error, uint pixelHeight, ResourcePath path, out Dictionary<char, RawGlyph>? rawGlyphsOut)
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











