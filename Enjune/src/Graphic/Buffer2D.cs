using System.Runtime.CompilerServices;
using Enjune.Misc;
using Buffer = System.Buffer;

namespace Enjune.Graphic;

public class Buffer2D<T>
{
    public readonly int Width;
    public readonly int Height;
    public readonly T[] Data;

    public Buffer2D(int width, int height)
    {
        Width = width;
        Height = height;
        Data = new T[width * height];
    }

    public Buffer2D(int width, int height, T[] inputData) : this(width, height)
    {
        if (inputData.Length != width * height){
            Logger.Warn(this, $"data length ({inputData.Length}) should be equal to w*h ({width * height})");
        }
        new Span<T>(inputData, 0, width * height).CopyTo(Data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] 
    public T GetAt(int x, int y) => Data[y * Width + x];
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetAt(int x, int y, T value) => Data[y * Width + x] = value;
    
    public void PasteFrom(Buffer2D<T> other, int xOffset, int yOffset)
    {
        if (xOffset < 0 || yOffset < 0)
        {
            Logger.Warn(this, $"position must be non negative: x={xOffset}; y={yOffset}");
            return;
        }
        if (xOffset + other.Width > Width || yOffset + other.Height > Height)
        {
            Logger.Warn(this, $"paste not in bounds: " +
                              $"must: {xOffset+other.Width} <= {Width} && {yOffset+other.Height} <= {Height}");
            return;
        }

        for (int x = 0; x < other.Width; x++)
        {
            for (int y = 0; y < other.Height; y++)
            {
                SetAt(x+xOffset, y+yOffset, other.GetAt(x, y));
            }
        }
    }
}