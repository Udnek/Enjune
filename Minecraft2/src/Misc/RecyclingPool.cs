using Enjune.Logger;
using Enjune.Misc;

namespace Minecraft2.Misc;


public class RecyclingPool<T>
{
    private readonly Queue<T> _queue = new();
    private readonly Func<T> _fabric;
    private uint _totalCreated = 0;
    
    /// <summary>
    /// Fabric will be called when pool is empty
    /// </summary>
    public RecyclingPool(Func<T> fabric) => _fabric = fabric;

    /// <summary>
    /// Adds item to the pool for later retrieve
    /// </summary>
    public void Recycle(T item) => _queue.Enqueue(item);

    /// <summary>
    /// Takes and removes item from pool is any. Creates new instance via fabric otherwise
    /// </summary>
    public T Take()
    {
        if (_queue.TryDequeue(out var result))
            return result;
        
        Logger.Info(this, $"Pool is empty, creating new number {_totalCreated}");
        _totalCreated += 1;
        return _fabric();
    }
}