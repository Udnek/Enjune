namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Array;

public sealed class Vbo<T> : AbstractBuffer<T>, IVbo where T : unmanaged
{
    public Vbo(int capacity) : base(BufferTarget.ArrayBuffer, capacity) { }
}