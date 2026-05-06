namespace OpenGLApi.Component.Buffer;

public sealed class Ebo(int capacity, bool final, int[]? initialData = null)
    : AbstractBuffer<int>(BufferTarget.ElementArrayBuffer, capacity, final, initialData);