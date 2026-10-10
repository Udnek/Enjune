using Enjune.Misc;

namespace Minecraft2.Misc;


public class RecyclingPool<T>
{
    private readonly Stack<T> _stack = new();
    private readonly Func<T> _fabric;
    
    /// <summary>
    /// Fabric will be called when pool is empty
    /// </summary>
    /// <param name="fabric"></param>
    public RecyclingPool(Func<T> fabric) => _fabric = fabric;

    /// <summary>
    /// Adds item to the pool for later retrieve
    /// </summary>
    /// <param name="item"></param>
    public void Recycle(T item) => _stack.Push(item);

    /// <summary>
    /// Takes and removes item from pool is any. Creates new instance via fabric otherwise
    /// </summary>
    /// <returns></returns>
    public T Take()
    {
        if (_stack.TryPop(out var result))
            return result;
        
        return _fabric();
    }
}