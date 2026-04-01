using OpenTK.Graphics.OpenGL4;

namespace Enjune.Graphic.OpenGL.Array;

public class VbosAndEbo
{
    private readonly int _vboMain;
    private readonly int _vboTexLayer;
    private readonly int _ebo;
    private readonly BufferUsageHint _usageHint;

    public VbosAndEbo(BufferUsageHint bufferUsageHint)
    {
        _usageHint = bufferUsageHint;
        
        _vboMain = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vboMain);
        
        _vboTexLayer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vboMain);
        
        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
    }
    
    public void PushMainVbo(FixedBuffer<float> buffer)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vboMain);
        GL.BufferData(BufferTarget.ArrayBuffer, buffer.Count * sizeof(float), buffer.Data, _usageHint);
    }
    
    
    
    public void PushEbo(FixedBuffer<int> buffer)
    {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, buffer.Count * sizeof(int), buffer.Data, _usageHint);
    }

    public void Destroy()
    {
        GL.DeleteBuffer(_vboMain);
        GL.DeleteBuffer(_ebo);
    }
}