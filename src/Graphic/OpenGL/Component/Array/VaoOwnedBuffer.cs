namespace Enjune.Graphic.OpenGL.Component.Array;

public abstract class VaoOwnedBuffer<T> : GLDisposable where T : unmanaged
{
    private readonly int _handle;
    protected readonly int ElementSize;
    protected readonly BufferTarget Target;

    public VaoOwnedBuffer(BufferTarget target, int capacity)
    {
        _handle = GL.GenBuffer();
        Target = target;
        unsafe
        {
            ElementSize = sizeof(T);
        }
        Bind();
        GL.BufferStorage(Target, capacity*ElementSize, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
    }

    public void Bind() => GL.BindBuffer(Target, _handle);
    public void Unbind() => GL.BindBuffer(Target, 0);
    
    public void BindAndPush(FixedBuffer<T> fixedBuffer)
    {
        Bind();
        GL.BufferSubData(Target, 0, fixedBuffer.Count*ElementSize, fixedBuffer.Data);
    }

    protected override void DisposeGLData() => GL.DeleteBuffer(_handle);
}