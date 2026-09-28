using Enjune.Misc;

namespace OpenGLApi.Component.Buffer;

public abstract class AbstractBuffer<T> : GlDisposable where T : unmanaged
{
    protected readonly int Handle;
    private readonly int _elementSize;
    private readonly BufferTarget _target;
    public int Capacity { get; private set; }
    public readonly bool Final;

    protected AbstractBuffer(BufferTarget target, int capacity, bool final, ReadOnlySpan<T> initialData = default)
    {
        if (capacity <= 0)
        {
            Logger.Error(this, "capacity must be positive");
            capacity = 1;
        }

        Final = final;
        Capacity = capacity;
        _target = target;
        Handle = GL.GenBuffer();
        Bind();
        unsafe { _elementSize = sizeof(T); }

        if (final) 
            GL.BufferStorage(_target, capacity*_elementSize, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
        else 
            GL.BufferData(_target, capacity*_elementSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        
        if (!initialData.IsEmpty) 
            BindAndPush(initialData);
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
        Logger.Info(this, $"capacity increased: {Capacity} -> {newCapacity}");
        Capacity = newCapacity;
    }
    
    public void Bind() => GL.BindBuffer(_target, Handle);

    public void BindAndPush(ReadOnlySpan<T> span)
    {
        Bind();
        unsafe
        {
            fixed (T* pointer = span)
            {
                GL.BufferSubData(_target, 0, span.Length*_elementSize, (IntPtr)pointer);
            }
        }
    }

    protected override void DisposeGlData() => GL.DeleteBuffer(Handle);
}