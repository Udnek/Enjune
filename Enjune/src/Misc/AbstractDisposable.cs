namespace Enjune.Misc;

public abstract class AbstractDisposable : IDisposable
{
    private bool _disposed = false;

    protected abstract void DisposeData();
    
    public void Dispose()
    {
        if (_disposed)
        {
            Logger.Warn(this, "Trying to dispose several times");
            return;
        }
        DisposeData();
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    ~AbstractDisposable()
    {
        if (_disposed) return;
        Logger.Warn(this, $"Dispose called only during finalizing; should call {nameof(Dispose)}() manually");
        Dispose();
    }
}