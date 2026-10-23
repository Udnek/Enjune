using System.Runtime.InteropServices;
using Enjune.Graphic.Api;
using Enjune.Misc;
using OpenGLApi.Generation;

namespace OpenGLApi.Component.Buffer;

/// <summary>
/// Shared storage between shaders, can be used to store per primitive data
/// Must be std430
/// </summary>
public sealed class Ssbo<T> : AbstractBuffer<T>, IUniformArray<T> where T : unmanaged
{
    private readonly int _binding;

    public Ssbo(int binding, int capacity, bool fixedSize) 
        : base(BufferTarget.ShaderStorageBuffer, capacity, fixedSize)
    {
        _binding = binding;
        Std.Std430.Check<T>()?.Log(this);
        Bind();
    }
    
    public override void Bind()
    {
        base.Bind();
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, _binding, Handle);
    }

    public void Push(ReadOnlySpan<T> data)
    {
        if (data.Length > Capacity) 
            Reallocate(data.Length);
        BindAndPush(data);
    }

    public T this[int p]
    {
        set => BindAndSetAt(p, value);
    }
}