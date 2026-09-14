using System.Collections;
using JetBrains.Annotations;

namespace Enjune.Misc;


public delegate void ListModificationAction<in T>(int index, T item);

public interface IObservableReadonlyList<out T> : IReadOnlyList<T>
{
    /// <summary>
    /// Subscribes to change. Unsubscriber should be used to unsubscribe and thus avoid memory leak
    /// </summary>
    /// <param name="action"></param>
    /// <returns>Unsubscriber to be called when action's owner is disposed</returns>
    [MustUseReturnValue]
    Unsubscriber AfterItemAdded(ListModificationAction<T> action);
    
    /// <summary>
    /// Subscribes to change. Unsubscriber should be used to unsubscribe and thus avoid memory leak
    /// </summary>
    /// <param name="action"></param>
    /// <returns>Unsubscriber to be called when action's owner is disposed</returns>
    [MustUseReturnValue]
    Unsubscriber AfterItemRemoved(ListModificationAction<T> action);
}

public class ObservableList<T> : IList<T>, IObservableReadonlyList<T>
{
    private readonly List<T> _list;
    private event ListModificationAction<T>? AfterItemAddedEvent;
    private event ListModificationAction<T>? AfterItemRemovedEvent;

    public ObservableList(int capacity = 0) => _list = new List<T>(capacity);
    public ObservableList(IEnumerable<T> collection) => _list = new List<T>(collection);

    /// <summary>
    /// Subscribes to change. But may cause memory leak, so use it only if delegate is disposed with value's owner
    /// </summary>
    /// <param name="action"></param>
    public void AfterItemAddedAsOwner(ListModificationAction<T> action) => AfterItemAddedEvent += action;
    
    /// <summary>
    /// Subscribes to change. But may cause memory leak, so use it only if delegate is disposed with value's owner
    /// </summary>
    /// <param name="action"></param>
    public void AfterItemRemovedAsOwner(ListModificationAction<T> action) => AfterItemRemovedEvent += action;

    public Unsubscriber AfterItemAdded(ListModificationAction<T> action)
    {
        AfterItemAddedEvent += action;
        return new Unsubscriber(() => AfterItemAddedEvent -= action);
    }
    public Unsubscriber AfterItemRemoved(ListModificationAction<T> action)
    {
        AfterItemRemovedEvent += action;
        return new Unsubscriber(() => AfterItemRemovedEvent -= action);
    }
    
    //
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    [System.Diagnostics.Contracts.Pure]
    public Span<T> AsSpan() => _list.AsSpan();
    
    public void Add(T item)
    {
        _list.Add(item);
        AfterItemAddedEvent?.Invoke(Count-1, item);
    }

    public void Clear()
    {
        for (var i = Count - 1; i >= 0; i--) 
            RemoveAt(i);
    }

    public bool Contains(T item) => _list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index < 0) return false;
        RemoveAt(index);
        return true;

    }

    public int Count => _list.Count;
    public bool IsReadOnly => false;

    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        AfterItemAddedEvent?.Invoke(index, item);
    }

    public void RemoveAt(int index)
    {
        var item = _list.ElementAt(index);
        _list.RemoveAt(index);
        AfterItemRemovedEvent?.Invoke(index, item);
    }

    public T this[int index]
    {
        get => _list[index];
        set
        {
            var oldValue = this[index];
            _list[index] = value;
            AfterItemRemovedEvent?.Invoke(index, oldValue);
            AfterItemAddedEvent?.Invoke(index, value);
        }
    }
}