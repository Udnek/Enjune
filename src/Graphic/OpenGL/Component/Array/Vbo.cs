namespace Enjune.Graphic.OpenGL.Component.Array;

public sealed class Vbo<T> : AbstractBuffer<T> where T : unmanaged
{
    public Vbo(int capacity) : base(BufferTarget.ArrayBuffer, capacity) { }
}