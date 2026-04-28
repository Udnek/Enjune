namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Buffer;


public interface IVbo
{
    public void Bind();
}

public sealed class Vbo<T> : AbstractBuffer<T>, IVbo where T : unmanaged
{
    public Vbo(int capacity) : base(BufferTarget.ArrayBuffer, capacity) { }
}