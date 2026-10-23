namespace OpenGLApi.Component.Buffer;

/// <summary>
/// Stores indexes to form primitives from VBO
/// </summary>
public sealed class Ebo(int capacity, bool fixedSize)
    : AbstractBuffer<int>(BufferTarget.ElementArrayBuffer, capacity, fixedSize);