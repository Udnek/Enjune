using Enjune.Misc;

namespace Enjune.Graphic.GraphicApi.OpenGL;

public abstract class GlDisposable : IDisposable
{
    private bool _disposed = false;

    protected abstract void DisposeGlData();
    
    public void Dispose()
    {
        if (_disposed) return;
        DisposeGlData();
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    ~GlDisposable()
    {
        if (_disposed) return;
        Logger.Warn(this, "dispose called only during finalizing; should call Dispose() manually");
        Dispose();
    }
}