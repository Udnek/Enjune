using System.Security.Cryptography;
using Enjune.Misc;
using StbImageSharp;

namespace Enjune.Graphic;

public sealed class ByteImage : IEquatable<ByteImage>
{
    public readonly int Width;
    public readonly int Height;
    public readonly Kind Type;
    public readonly byte[] Data;
    
    public static ByteImage Empty(int width, int height, Kind type) 
        => new(width, height, type, new byte[width * height * type.Depth]);
    
    public ByteImage(int width, int height, Kind type, byte[] data)
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

    public ByteImage Alpha8ToRgba32(byte r =  255, byte g = 255, byte b = 255)
    {
        if (Type.Depth != 1)
        {
            Logger.Warn(this, $"conversion only supports 1d images, but got: {Type.Depth}");
            return this;
        }
        byte[] newData = new byte[Width * Height * 4];
        for (int i = 0; i < Width * Height * 4; i+=4)
        {
            newData[i] = r;
            newData[i+1] = g;
            newData[i+2] = b;
            newData[i+3] = Data[i/4];
        }
        return new ByteImage(Width, Height, Kind.Rgba32, newData);
    }

    public bool Equals(ByteImage? other) 
        => other?.GetHashCode() == GetHashCode();

    public override bool Equals(object? obj) 
        => (obj as ByteImage)?.GetHashCode() == GetHashCode();

    public override int GetHashCode() 
        => HashCode.Combine(Width, Height, Type, SHA1.HashData(Data));

    public readonly record struct Kind(int Depth, ColorComponents StbType)
    {
        public static readonly Kind Rgba32 = new(4, ColorComponents.RedGreenBlueAlpha);
        public static readonly Kind Rgb24 = new(3, ColorComponents.RedGreenBlue);
        public static readonly Kind GreyAlpha16 = new(2, ColorComponents.GreyAlpha);
        public static readonly Kind Alpha8 = new(1, ColorComponents.Grey);

        public static Kind FromStb(ColorComponents comps)
        {
            return comps switch
            {
                ColorComponents.Grey => Alpha8,
                ColorComponents.GreyAlpha => GreyAlpha16,
                ColorComponents.RedGreenBlue => Rgb24,
                ColorComponents.RedGreenBlueAlpha => Rgba32,
                ColorComponents.Default => throw new ArgumentOutOfRangeException(nameof(comps), comps, null),
                _ => throw new ArgumentOutOfRangeException(nameof(comps), comps, null)
            };
        }
        
        public static Kind OfDepth(int depth)
        {
            return depth switch
            {
                1 => Alpha8,
                2 => GreyAlpha16,
                3 => Rgb24,
                4 => Rgba32,
                _ => throw new ArgumentOutOfRangeException(nameof(depth), depth, null)
            };
        }
    }
}