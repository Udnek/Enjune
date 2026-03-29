using System.Runtime.CompilerServices;

namespace Enjune.Graphic.OpenGL.Array;

public class SmartBuffer<T>(int capacity) where T : unmanaged
{
    public readonly T[] Data = new T[capacity];
    private int _pointer = 0;

    public int Count => _pointer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Put(T value) => Data[_pointer++] = value;
    public void Clear() => _pointer = 0;
}