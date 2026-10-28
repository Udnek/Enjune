using Enjune.Logger;
using Enjune.Misc;

namespace OpenGLApi.Component.Buffer;

public interface IGlBuffer : IDisposable;

public abstract class AbstractBuffer<T> : GlDisposable, IGlBuffer where T : unmanaged
{
    protected readonly int Handle;
    private readonly int _elementSize;
    private readonly BufferTarget _target;
    public int Capacity { get; private set; }
    public int Count { get; private set; } = 0;
    public readonly bool FixedSize;

    protected AbstractBuffer(BufferTarget target, int capacity, bool fixedSize)
    {
        if (capacity <= 0)
        {
            Logger.Error(this, $"Capacity must be positive; got {capacity}");
            throw new Exception($"Capacity must be positive; got {capacity}");
        }

        FixedSize = fixedSize;
        Capacity = capacity;
        _target = target;
        Handle = GL.GenBuffer();
        OpenGlApi.CheckGlError(this);
        unsafe { _elementSize = sizeof(T); }
        Initialize();
    }

    private void Initialize()
    {
        Bind();

        if (FixedSize) 
            GL.BufferStorage(_target, Capacity*_elementSize, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
        else 
            GL.BufferData(_target, Capacity*_elementSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        OpenGlApi.CheckGlError(this);
    }

    public void Reallocate(int newCapacity)
    {
        if (FixedSize)
        {
            Logger.Error(this, "trying to reallocate fixed-size buffer");
            return;
        }
        if (newCapacity <= 0)
        {
            Logger.Error(this, $"capacity must be positive, got {newCapacity}");
            return;
        }
        Bind();
        GL.BufferData(_target, newCapacity*_elementSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        OpenGlApi.CheckGlError(this);
        Logger.Info(this, $"capacity changed: {Capacity} -> {newCapacity}");
        Capacity = newCapacity;
        Count = 0;
    }

    public virtual void Bind() => DynamicBindings.BindBuffer(_target, Handle);

    public void BindAndSetAt(int pos, T val)
    {
        Bind();
        if (pos < 0)
        {
            Logger.Error(this, $"Pos must be >= 0, got {pos}");
            return;
        }

        if (pos >= Capacity)
        {
            Logger.Error(this, $"Pos must be < capacity, got {pos}");
            return;
        }
        
        GL.BufferSubData(_target, pos * _elementSize, _elementSize, ref val);
        OpenGlApi.CheckGlError(this);
    }
    
    public void BindAndPush(ReadOnlySpan<T> span)
    {
        Bind();
        if (span.Length > Capacity)
        {
            Logger.Error(this, $"Trying to push {span.Length} elements to buffer with {Capacity} capacity");
            return;
        }

        Count = span.Length; 
        
        unsafe
        {
            fixed (T* pointer = span)
            {
                GL.BufferSubData(_target, 0, span.Length*_elementSize, (IntPtr)pointer);
            }
        }
        OpenGlApi.CheckGlError(this);
    }

    protected override void DisposeGlData() => GL.DeleteBuffer(Handle);
}