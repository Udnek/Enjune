using Enjune.Misc;

namespace Minecraft2.Misc;


public class RecyclingPool<T>
{
    public readonly Stack<T> Stack = new();
    private readonly Func<T> _fabric;
    
    /// <summary>
    /// Fabric will be called when pool is empty
    /// </summary>
    /// <param name="fabric"></param>
    public RecyclingPool(Func<T> fabric)
    {
        _fabric = fabric;
    }

    /// <summary>
    /// Adds item to the pool for later retrieve
    /// </summary>
    /// <param name="item"></param>
    public void Recycle(T item)
    {
        Logger.Info(this, $"Item recycled; size {Stack.Count} -> {Stack.Count+1}");
        Stack.Push(item);
    }

    /// <summary>
    /// Takes and removes item from pool is any. Creates new instance via fabric otherwise
    /// </summary>
    /// <returns></returns>
    public T Take()
    {
        if (Stack.TryPop(out var result))
        {
            Logger.Info(this, $"Item taken; size {Stack.Count+1} -> {Stack.Count}");
            return result;
        }

        Logger.Info(this, "Stack is empty; new item created");
        return _fabric();
    }
}