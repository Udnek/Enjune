using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Component;

public sealed class Ebo: VaoOwnedBuffer<int>
{
    public Ebo(int capacity) : base(BufferTarget.ElementArrayBuffer, capacity) { }
}