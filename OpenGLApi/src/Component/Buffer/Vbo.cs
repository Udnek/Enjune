namespace OpenGLApi.Component.Buffer;


public interface IVbo
{
    public void Bind();
}

public sealed class Vbo<T>(int capacity, bool final, T[]? initialData = null)
    : AbstractBuffer<T>(BufferTarget.ArrayBuffer, capacity, final, initialData), IVbo
    where T : unmanaged;