using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Array;

public class VboAndEbo
{
    private readonly int _vbo;
    private readonly int _ebo;
    private readonly BufferUsageHint _usageHint;

    public VboAndEbo(BufferUsageHint bufferUsageHint)
    {
        _usageHint = bufferUsageHint;
        
        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        
        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
    }
    
    public void PushVbo(FixedBuffer<float> buffer)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, buffer.Count * sizeof(float), buffer.Data, _usageHint);
    }
    
    public void PushEbo(FixedBuffer<int> buffer)
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, buffer.Count * sizeof(int), buffer.Data, _usageHint);
    }

    public void Destroy()
    {
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
    }
}