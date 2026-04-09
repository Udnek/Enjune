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
    public record struct RawGlyph
    {
        public uint Width;
        public uint Height;
        public int BearingX;
        public int BearingY;
        public float Advance;
        public byte[] Buffer;
        
        public RawGlyph(uint Width, uint Height, int BearingX, int BearingY, float Advance, byte[] Buffer)
        {
            this.Width = Width;
            this.Height = Height;
            this.BearingX = BearingX;
            this.BearingY = BearingY;
            this.Advance = Advance;
            this.Buffer = Buffer;
        }
    }
    
    public static unsafe void Load(out string? error, uint pixelHeight, ResourcePath path, out ByteImage? image, out Dictionary<char, RawGlyph>? rawGlyphs)
    {
        rawGlyphs = null;
        image = null;
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
            FT_Done_FreeType(ft);
            error = loadBytesError;
            return;
        }
        
        IntPtr fontMemory = Marshal.AllocHGlobal(fontBytes.Length);
        Marshal.Copy(fontBytes, 0, fontMemory, fontBytes.Length);
        
        FT_FaceRec_* face;
        var faceError = FT_New_Memory_Face(ft, (byte*)fontMemory, fontBytes.Length, 0, &face);
        Marshal.FreeHGlobal(fontMemory);
        if (faceError != FT_Error.FT_Err_Ok)
        {
            FT_Done_FreeType(ft);
            error = faceError.ToString();
            return;
        }

        FT_Set_Pixel_Sizes(face, 0, pixelHeight);
        
        rawGlyphs = new Dictionary<char, RawGlyph>();
        for (byte charI = 0; charI < 128; charI++)
        {
            var ch = (char) charI;
            var loadError = FT_Load_Char(face, ch, FT_LOAD.FT_LOAD_RENDER);
            if (loadError != FT_Error.FT_Err_Ok)
            {
                FT_Done_Face(face);
                FT_Done_FreeType(ft);
                error = loadError.ToString();
                return;
            }
            
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
            if (rawGlyph.Width == 0 || rawGlyph.Height == 0)
            {
                Logger.Warn(typeof(FontLoader), $"char '{ch}' ({(byte)ch}) has zero size: {rawGlyph}; defaulting to 1x1");
                rawGlyph.Width = 1;
                rawGlyph.Height = 1;
                rawGlyph.Buffer = new byte[1];
            }
            rawGlyphs[ch] = rawGlyph;
        }
        
        FT_Done_Face(face);
        FT_Done_FreeType(ft);
        
        // foreach (var (c, glyph) in rawGlyphs)
        // {
        //     Logger.Log(this, $"{c} ({(byte) c}) = {glyph}");
        // }

        PackingRectangle[] rectangles;
        {
            var rectanglesList = new List<PackingRectangle>(rawGlyphs.Count);
            foreach (var (ch, glyph) in rawGlyphs)
            {
                rectanglesList.Add(new PackingRectangle(0, 0, glyph.Width, glyph.Height, id:ch));
            }
            rectangles = rectanglesList.ToArray();
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

        error = null;
        image = new ByteImage(atlasSize, atlasSize, ByteImage.ImType.Grey8, atlasBuffer.Data);
    }
}











