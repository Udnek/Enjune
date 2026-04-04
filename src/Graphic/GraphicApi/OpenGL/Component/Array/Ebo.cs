namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Array;

public sealed class Ebo: AbstractBuffer<int>
{
    public Ebo(int capacity) : base(BufferTarget.ElementArrayBuffer, capacity) { }
}