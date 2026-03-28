using System.Runtime.CompilerServices;

namespace Enjune.Graphic.OpenGL.Arrays;

public abstract class SmartBuffer<T>(int capacity) where T : unmanaged
{
    protected readonly T[] Values = new T[capacity];
    // make private and introduce Count
    public int Pointer; 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Put(T value) => Values[Pointer++] = value;
    public void Clear() => Pointer = 0;

    public abstract void BindAndPush();
}