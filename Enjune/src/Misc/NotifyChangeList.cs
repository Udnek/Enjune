using System.Collections;

namespace Enjune.Misc;


public interface INotifyChangeReadonlyList<T> : IReadOnlyList<T>
{
    event Action<T> OnElementAdded;
    event Action<T> OnElementRemoved;
}

public class NotifyChangeList<T>(int capacity = 4) : IList<T>, INotifyChangeReadonlyList<T>
{
    private readonly List<T> _list = new(capacity);
    public event Action<T>? OnElementAdded;
    public event Action<T>? OnElementRemoved;

    //
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    public void Add(T item)
    {
        _list.Add(item);
        OnElementAdded?.Invoke(item);
    }

    public void Clear()
    {
        if (_list.Count == 0)
            return;
        _list.ForEach(i => OnElementRemoved?.Invoke(i));
        _list.Clear();
    }

    public bool Contains(T item) => _list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        var removed = _list.Remove(item);
        if (removed) 
            OnElementRemoved?.Invoke(item);
        return removed;
    }

    public int Count => _list.Count;
    public bool IsReadOnly => false;

    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        OnElementAdded?.Invoke(item);
    }

    public void RemoveAt(int index)
    {
        var item = _list.ElementAt(index);
        _list.RemoveAt(index);
        OnElementRemoved?.Invoke(item);
    }

    public T this[int index]
    {
        get => _list[index];
        set
        {
            OnElementRemoved?.Invoke(this[index]);
            _list[index] = value;
            OnElementAdded?.Invoke(this[index]);
        }
    }
}