using System.Runtime.InteropServices;
using Enjune.Misc;

namespace OpenGLApi.Component.Buffer;

/// <summary>
/// Shared storage between shaders, can be used to store per primitive data
/// </summary>
public sealed class Ssbo<T> : AbstractBuffer<T> where T : unmanaged
{
    private readonly int _binding;

    public Ssbo(int binding, int capacity, bool fixedSize) 
        : base(BufferTarget.ShaderStorageBuffer, capacity, fixedSize)
    {
        _binding = binding;
        SsboUtils.CheckStd430<T>(true, 0)?.Log(this);
        Bind();
    }
    
    public override void Bind()
    {
        base.Bind();
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, _binding, Handle);
    }
}