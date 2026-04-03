namespace Enjune.Graphic.OpenGL.Component.Array;

public class Ssbo<T> : AbstractBuffer<T> where T : unmanaged
{
    public Ssbo(int capacity) : base(BufferTarget.ShaderStorageBuffer, capacity)
    {
        unsafe
        {
            var size = sizeof(T);
            if ()
        }
    }
}