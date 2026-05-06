using Enjune.Misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGLApi.Component.Buffer;

public sealed class Fbo : GlDisposable
{
    public static Vector2i SizeOfDefault = new(1, 1);
    private Vector2i _size;
    private readonly int _handle;
    
    public Fbo(Vector2i size)
    {
        _size = size;
        _handle = GL.GenFramebuffer();
    }
    
    public void CheckStatus()
    {
        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete) 
            Logger.Error(this, $"framebuffer failed: {Enum.GetName(status)}");
    }

    public void SetEmptyColorBuffer()
    {
        Bind();
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
    }

    public void Resize(Vector2i size) => _size = size;
    
    public void Bind()
    {
        GL.Viewport(0, 0, _size.X, _size.Y);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
    }

    public static void BindDefault()
    {
        GL.Viewport(0, 0, SizeOfDefault.X, SizeOfDefault.Y);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    protected override void DisposeGlData() => GL.DeleteFramebuffer(_handle);
}