using System.Runtime.CompilerServices;

namespace Enjune.Graphic.OpenGL.Arrays;

public abstract class SmartBuffer<T>(int capacity) where T : unmanaged
{
    protected readonly T[] Values = new T[capacity];
    private int _pointer = 0;

    public int Count => _pointer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Put(T value) => Values[_pointer++] = value;
    public void Clear() => _pointer = 0;

    public abstract void BindAndPush();
}