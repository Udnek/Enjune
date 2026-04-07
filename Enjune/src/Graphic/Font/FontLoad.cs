using System.Runtime.InteropServices;
using Enjune.File;
using FreeTypeSharp;
using static FreeTypeSharp.FT;

namespace Enjune.Graphic.Font;

public class FontLoad
{
   
    public Dictionary<char, Glyph> asd = new ();
    
    public struct Glyph
    {
        byte[] bitmap;
        int width;
        int height;
        int bearingX;
        int bearingY;
        int advance;
    }
    
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

        FT_Set_Pixel_Sizes(face, 0, 32);
        
        for (uint i = 0; i < 128; i++)
        {
            var loadError = FT_Load_Char(face, (char)i, FT_LOAD.FT_LOAD_RENDER);
            if (loadError != FT_Error.FT_Err_Ok) return loadError.ToString();

            byte[] buffer = *face->glyph->bitmap.buffer;
        }
        

        // free
        Marshal.FreeHGlobal(fontMemory);
        FT_Done_Face(face);
        FT_Done_FreeType(ft);
        return null;
    }
}