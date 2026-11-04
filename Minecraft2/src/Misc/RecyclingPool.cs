using Enjune.Logger;
using Enjune.Misc;

namespace Minecraft2.Misc;


public class RecyclingPool<T>
{
    private readonly Queue<T> _queue = new();
    private readonly Func<T> _fabric;
    public uint TotalCreated { get; private set; } = 0; // amount of items pool created through fabric

    /// <summary>
    /// Fabric will be called when pool is empty
    /// </summary>
    public RecyclingPool(Func<T> fabric) => _fabric = fabric;

    /// <summary>
    /// Adds item to the pool for later retrieve
    /// </summary>
    public void Recycle(T item)
    {
        Logger.Debug(this, "Got recycled item");
        _queue.Enqueue(item);
    }

    /// <summary>
    /// Takes and removes item from pool is any. Creates new instance via fabric otherwise
    /// </summary>
    public T Take()
    {
        if (_queue.TryDequeue(out var result))
            return result;
        
        Logger.Debug(this, $"Pool is empty, creating new number {TotalCreated}");
        TotalCreated += 1;
        return _fabric();
    }
}