using System.Security.Cryptography;
using Enjune.Misc;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.PixelFormats;
using StbImageSharp;

namespace Enjune.Graphic;

public sealed class ByteImage : IEquatable<ByteImage>
{
    public readonly int Width;
    public readonly int Height;
    public readonly Format Type;
    public readonly byte[] Data;

    public Vector2i Size => (Width, Height);
    
    public static ByteImage Empty(int width, int height, Format type) 
        => new(width, height, type, new byte[width * height * type.Depth]);
    
    public ByteImage(int width, int height, Format type, byte[] data)
    {
        if (width <= 0 || height <= 0)
            throw new Exception($"Size must be positive, got {width}x{height}");
        
        if (width * height * type.Depth != data.Length)
        {
            Logger.Warn(this, $"Data length: {data.Length}, expected (w*h*d): {width * height * type.Depth}");
            data = new byte[width * height*type.Depth];
        }
        
        Data = data;
        Width = width;
        Height = height;
        Type = type;
    }

    public ByteImage ToRgba32()
    {
        if (Type.Depth == 4) // already rgba
            return this;
        byte[] newData = new byte[Width * Height * 4];
        var depth = Type.Depth;
        for (int i = 0; i < Width * Height * 4; i+=4)
        {
            switch (depth) // I hope compiler optimize this switch 
            {
                // gray
                case 1:
                    newData[i] = Data[i/4];
                    newData[i+1] = Data[i/4];
                    newData[i+2] = Data[i/4];
                    newData[i+3] = 255;
                    break;
                // gray + alpha
                case 2:
                    newData[i] = Data[i/4];
                    newData[i+1] = Data[i/4];
                    newData[i+2] = Data[i/4];
                    newData[i+3] = Data[i/4 + 1];
                    break;
                // rgb
                case 3:
                    newData[i] = Data[i/4];
                    newData[i+1] = Data[i/4 +1];
                    newData[i+2] = Data[i/4 +2];
                    newData[i+3] = 255;
                    break;
                default:
                    throw new Exception();
            }
        }

        return new ByteImage(Width, Height, Format.Rgba32, newData);
    }

    public bool Equals(ByteImage? other) 
        => other?.GetHashCode() == GetHashCode();

    public override bool Equals(object? obj) 
        => (obj as ByteImage)?.GetHashCode() == GetHashCode();

    public override int GetHashCode() 
        => HashCode.Combine(Width, Height, Type, SHA1.HashData(Data));

    public readonly record struct Format(int Depth, ColorComponents StbType)
    {
        public static readonly Format Rgba32 = new(4, ColorComponents.RedGreenBlueAlpha);
        public static readonly Format Rgb24 = new(3, ColorComponents.RedGreenBlue);
        public static readonly Format GreyAlpha16 = new(2, ColorComponents.GreyAlpha);
        public static readonly Format Grey8 = new(1, ColorComponents.Grey);

        public static Format FromStb(ColorComponents comps)
        {
            return comps switch
            {
                ColorComponents.Grey => Grey8,
                ColorComponents.GreyAlpha => GreyAlpha16,
                ColorComponents.RedGreenBlue => Rgb24,
                ColorComponents.RedGreenBlueAlpha => Rgba32,
                ColorComponents.Default => throw new ArgumentOutOfRangeException(nameof(comps), comps, null),
                _ => throw new ArgumentOutOfRangeException(nameof(comps), comps, null)
            };
        }
        
        public static Format OfDepth(int depth)
        {
            return depth switch
            {
                1 => Grey8,
                2 => GreyAlpha16,
                3 => Rgb24,
                4 => Rgba32,
                _ => throw new ArgumentOutOfRangeException(nameof(depth), depth, null)
            };
        }
    }
}