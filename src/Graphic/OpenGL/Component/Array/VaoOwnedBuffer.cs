using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Component;

public abstract class VaoOwnedBuffer<T> : GLDisposable where T : unmanaged
{
    private readonly int _handle;
    protected readonly int _elementSize;
    protected readonly BufferTarget _target;

    public VaoOwnedBuffer(BufferTarget target, int capacity)
    {
        _handle = GL.GenBuffer();
        _target = target;
        unsafe
        {
            _elementSize = sizeof(T);
        }
        Bind();
        GL.BufferStorage(_target, _elementSize*capacity, IntPtr.Zero, BufferStorageFlags.MapWriteBit);
        Unbind();
    }

    public void Bind() => GL.BindBuffer(_target, _handle);
    public void Unbind() => GL.BindBuffer(_target, 0);
    
    public void BindAndPush(FixedBuffer<T> fixedBuffer)
    {
        Bind();
        GL.BufferSubData(_target, 0, fixedBuffer.Count*_elementSize, fixedBuffer.Data);
        Unbind();
    }

    protected override void DisposeGLData() => GL.DeleteBuffer(_handle);
}