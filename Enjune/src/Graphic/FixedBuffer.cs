using System.Runtime.CompilerServices;

namespace Enjune.Graphic;

public class FixedBuffer<T>(int capacity)
{
    public readonly T[] Data = new T[capacity];
    private int _pointer = 0;

    public int Count => _pointer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Put(T value) => Data[_pointer++] = value;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Put(T[] values)
    {
        values.CopyTo(Data, _pointer);
        _pointer += values.Length;
    }

    public void Clear() => _pointer = 0;
}