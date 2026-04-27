namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Array;

public abstract class AbstractBuffer<T> : GLDisposable where T : unmanaged
{
    protected readonly int Handle;
    protected readonly int ElementSize;
    protected readonly BufferTarget Target;

    public AbstractBuffer(BufferTarget target, int capacity)
    {
        Handle = GL.GenBuffer();
        Target = target;
        unsafe
        {
            ElementSize = sizeof(T);
        }
        Bind();
        GL.BufferStorage(Target, capacity*ElementSize, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
    }

    public void Bind() => GL.BindBuffer(Target, Handle);
    public void Unbind() => GL.BindBuffer(Target, 0);
    
    public void BindAndPush(FixedBuffer<T> fixedBuffer)
    {
        Bind();
        GL.BufferSubData(Target, 0, fixedBuffer.Count*ElementSize, fixedBuffer.Data);
    }
    
    public void BindAndPush(T[] array)
    {
        Bind();
        GL.BufferSubData(Target, 0, array.Length*ElementSize, array);
    }

    protected override void DisposeGLData() => GL.DeleteBuffer(Handle);
}