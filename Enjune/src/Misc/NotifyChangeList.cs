using System.Collections;

namespace Enjune.Misc;


public interface INotifyChangeReadonlyList<T> : IReadOnlyList<T>
{
    event Action<T> AfterElementAdded;
    event Action<T> AfterElementRemoved;

    public void ForEach(Action<T> action);
}

public class NotifyChangeList<T> : IList<T>, INotifyChangeReadonlyList<T>
{
    private readonly List<T> _list;

    public NotifyChangeList(int capacity = 0) => _list = new List<T>(capacity);

    public NotifyChangeList(IEnumerable<T> collection) => _list = new List<T>(collection);

    public event Action<T>? AfterElementAdded;
    public event Action<T>? AfterElementRemoved;

    //
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    public void ForEach(Action<T> action) => _list.ForEach(action);

    public void Add(T item)
    {
        _list.Add(item);
        AfterElementAdded?.Invoke(item);
    }

    public void Clear()
    {
        if (_list.Count == 0)
            return;
        var oldValues = new T[_list.Count];
        _list.CopyTo(oldValues);
        _list.Clear();
        oldValues.ForEach(i => AfterElementRemoved?.Invoke(i));
    }

    public bool Contains(T item) => _list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        var removed = _list.Remove(item);
        if (removed) 
            AfterElementRemoved?.Invoke(item);
        return removed;
    }

    public int Count => _list.Count;
    public bool IsReadOnly => false;

    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        AfterElementAdded?.Invoke(item);
    }

    public void RemoveAt(int index)
    {
        var item = _list.ElementAt(index);
        _list.RemoveAt(index);
        AfterElementRemoved?.Invoke(item);
    }

    public T this[int index]
    {
        get => _list[index];
        set
        {
            var oldValue = this[index];
            _list[index] = value;
            AfterElementRemoved?.Invoke(oldValue);
            AfterElementAdded?.Invoke(value);
        }
    }
}