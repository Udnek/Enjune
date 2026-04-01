namespace Enjune.Graphic.OpenGL.Component;

public abstract class GLDisposable : IDisposable
{
    private bool _disposed = false;

    protected abstract void DisposeGLData();
    
    public void Dispose()
    {
        if (_disposed) return;
        DisposeGLData();
        _disposed = true;
    }

    ~GLDisposable()
    {
        if (_disposed) return;
        Logger.Warn(this, "dispose called only during finalizing; should call Dispose() manually");
        Dispose();
    }
}