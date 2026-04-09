using Enjune.Misc;
using StbImageSharp;

namespace Enjune.Graphic.Asset;

public sealed class ByteImage
{
    public readonly int Width;
    public readonly int Height;
    public readonly ImType Type;
    public readonly byte[] Data;
    
    public ByteImage(int width, int height, ImType type, byte[] data)
    {
        if (width * height * type.Depth != data.Length)
        {
            Logger.Warn(this, $"data length: {data.Length}; expected (w*h*d): {width * height * type.Depth}");
            data = new byte[width * height*type.Depth];
        }
        Data = data;
        Width = width;
        Height = height;
        Type = type;
    }

    public static ByteImage Empty(int width, int height, ImType type) 
        => new(width, height, type, new byte[width * height * type.Depth]);

    // public override bool Equals(object? obj)
    // {
    //     if (obj is null) return false;
    //     if (obj is not ByteImage other) return false;
    //     
    //     return ype == other.Type && Data.SequenceEqual(other.Data);
    // }

    public struct ImType
    {
        public readonly int Depth;
        public readonly ColorComponents StbType;
        
        public static readonly ImType Rgba32 = new ImType(4 ,ColorComponents.RedGreenBlueAlpha);
        public static readonly ImType Rgb24 = new ImType(3 ,ColorComponents.RedGreenBlue);
        public static readonly ImType Grey8 = new ImType(1 ,ColorComponents.Grey);

        private ImType(int depth, ColorComponents stbType)
        {
            Depth = depth;
            StbType = stbType;
        }
    }
}