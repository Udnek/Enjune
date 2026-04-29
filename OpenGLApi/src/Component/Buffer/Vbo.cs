namespace OpenGLApi.Component.Buffer;


public interface IVbo
{
    public void Bind();
}

public sealed class Vbo<T>(int capacity, T[]? initialData = null)
    : AbstractBuffer<T>(BufferTarget.ArrayBuffer, capacity, initialData), IVbo
    where T : unmanaged;