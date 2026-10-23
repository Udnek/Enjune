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

    public ByteImage Alpha8ToRgba32(byte r =  255, byte g = 255, byte b = 255)
    {
        if (Type.Depth != 1)
        {
            Logger.Warn(this, $"Conversion only supports 1d images, but got: {Type.Depth}");
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
        public static readonly Format Alpha8 = new(1, ColorComponents.Grey);

        public static Format FromStb(ColorComponents comps)
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
        
        public static Format OfDepth(int depth)
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