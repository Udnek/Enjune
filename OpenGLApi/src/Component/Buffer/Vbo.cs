namespace OpenGLApi.Component.Buffer;

/// <summary>
/// Stores vertex data
/// </summary>
public sealed class Vbo<T>(int capacity, bool fixedSize)
    : AbstractBuffer<T>(BufferTarget.ArrayBuffer, capacity, fixedSize)
    where T : unmanaged;