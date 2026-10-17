using Enjune.Graphic.Api;

namespace OpenGLApi.Component.Buffer;

/// <summary>
/// Stores global variables, must be std140
/// </summary>
/// <typeparam name="T"></typeparam>
public class Ubo<T> : GlDisposable, IGlBuffer, IUniform<T> where T : unmanaged
{
    private const BufferTarget Target = BufferTarget.UniformBuffer;
    private readonly int _handle;
    private readonly int _binding;
    private readonly int _byteSize;

    public Ubo(int binding)
    {
        _handle = GL.GenBuffer();
        _binding = binding;
        unsafe { _byteSize = sizeof(T); }
        Bind();
        GL.BufferStorage(Target, _byteSize, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
        OpenGlApi.CheckGlError(this);
    }
    
    public void BindAndPush(T val)
    {
        Bind();
        GL.BufferSubData(Target, 0, _byteSize, ref val);
    }
    
    public void Bind()
    {
        DynamicBindings.BindBuffer(Target, _handle);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, _binding, _handle);
    }
    
    protected override void DisposeGlData() => GL.DeleteBuffer(_handle);
    
    public void Set(T value) => BindAndPush(value);
}