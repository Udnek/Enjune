namespace Enjune.Misc;

public abstract class AbstractDisposable : IDisposable
{
    private bool _disposed = false;

    protected abstract void DisposeData();
    
    public void Dispose()
    {
        if (_disposed) return;
        DisposeData();
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    ~AbstractDisposable()
    {
        if (_disposed) return;
        Logger.Warn(this, "dispose called only during finalizing; should call Dispose() manually");
        Dispose();
    }
}