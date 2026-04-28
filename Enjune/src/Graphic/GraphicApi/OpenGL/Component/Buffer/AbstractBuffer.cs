namespace Enjune.Graphic.GraphicApi.OpenGL.Component.Buffer;

public abstract class AbstractBuffer<T> : GlDisposable where T : unmanaged
{
    protected readonly int Handle;
    private readonly int _elementSize;
    private readonly BufferTarget _target;

    public AbstractBuffer(BufferTarget target, int capacity)
    {
        Handle = GL.GenBuffer();
        _target = target;
        unsafe
        {
            _elementSize = sizeof(T); 
        }
        Bind();
        GL.BufferStorage(_target, capacity*_elementSize, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
    }

    public void Bind() => GL.BindBuffer(_target, Handle);
    public void Unbind() => GL.BindBuffer(_target, 0);
    
    public void BindAndPush(FixedBuffer<T> fixedBuffer)
    {
        Bind();
        GL.BufferSubData(_target, 0, fixedBuffer.Count*_elementSize, fixedBuffer.Data);
    }
    
    public void BindAndPush(T[] array)
    {
        Bind();
        GL.BufferSubData(_target, 0, array.Length*_elementSize, array);
    }

    protected override void DisposeGlData() => GL.DeleteBuffer(Handle);
}