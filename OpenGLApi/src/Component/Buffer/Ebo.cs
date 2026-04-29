namespace OpenGLApi.Component.Buffer;

public sealed class Ebo(int capacity, int[]? initialData = null)
    : AbstractBuffer<int>(BufferTarget.ElementArrayBuffer, capacity, initialData);