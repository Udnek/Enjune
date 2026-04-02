namespace Enjune.Graphic.OpenGL.Component.Array;

public sealed class Vbo<T> : VaoOwnedBuffer<T> where T : unmanaged
{
    public Vbo(int capacity) : base(BufferTarget.ArrayBuffer, capacity) { }
    
    public void BindAndPush<TT>(FixedBuffer<TT> fixedBuffer) where TT : unmanaged
    {
        Bind();
        unsafe
        {
            GL.BufferSubData(Target, 0, fixedBuffer.Count*sizeof(TT), fixedBuffer.Data);
        }
    }
}