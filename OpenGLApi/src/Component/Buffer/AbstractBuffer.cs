using Enjune.Graphic;
using Enjune.Misc;

namespace OpenGLApi.Component.Buffer;

public abstract class AbstractBuffer<T> : GlDisposable where T : unmanaged
{
    protected readonly int Handle;
    private readonly int _elementSize;
    private readonly BufferTarget _target;
    public readonly int Capacity;
    public readonly bool Final;

    protected AbstractBuffer(BufferTarget target, int capacity, bool final, T[]? initialData = null)
    {
        if (capacity <= 0)
        {
            Logger.Error(this, "capacity must be positive");
            capacity = 1;
        }
        Capacity = capacity;
        _target = target;
        Handle = GL.GenBuffer();
        Bind();
        unsafe { _elementSize = sizeof(T); }

        if (final) 
            GL.BufferStorage(_target, capacity*_elementSize, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
        else 
            GL.BufferData(_target, capacity*_elementSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        
        if (initialData != null) BindAndPush(initialData);
    }

    public void Reallocate(int newCapacity)
    {
        if (Final)
        {
            Logger.Error(this, "trying to reallocate final buffer");
            return;
        }
        if (newCapacity <= 0)
        {
            Logger.Error(this, "capacity must be positive");
            newCapacity = 1;
        }
        Bind();
        GL.BufferData(_target, newCapacity*_elementSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        Logger.Log(this, $"capacity increased: {Capacity} -> {newCapacity}");
    }
    
    public void Bind() => GL.BindBuffer(_target, Handle);
    
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