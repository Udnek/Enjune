using System.Collections;
using System.ComponentModel;

namespace Enjune.Physics;

/* Practically, it's just a compact way to handle two dictionaries.
 This implementation does not add any optimizations and automatizations,
 nor does it handle any exceptions. It is to be expanded upon if needed. */
public class BDDictionary<T1, T2> where T2 : notnull where T1 : notnull
{
    private readonly Dictionary<T1, T2> _forward = new();
    private readonly Dictionary<T2, T1> _backward = new();

    public void Add(T1 key, T2 value)
    {
        _forward.Add(key, value);
        _backward.Add(value, key);
    }

    public T2 this[T1 key]
    {
        get => _forward[key];
        set
        {
            if (ContainsKey(key)){ DeleteByKey(key);}
            if (ContainsValue(value)){ DeleteByValue(value);}
            _forward[key] = value;
            _backward[value] = key;
        }
    }
    public T1 this[T2 key]
    {
        get => _backward[key];
        set
        {
            if (ContainsKey(value)){ DeleteByKey(value);}
            if (ContainsValue(key)){ DeleteByValue(key);}
            _backward[key] = value;
            _forward[value] = key;
        }
    }
    
    public void DeleteByKey(T1 key)
    {
        _backward.Remove(_forward[key]);
        _forward.Remove(key);
    }

    public void DeleteByValue(T2 value)
    {
        _forward.Remove(_backward[value]);
        _backward.Remove(value);
    }

    public bool ContainsKey(T1 key)
    {
        return _forward.ContainsKey(key);
    }

    public bool ContainsValue(T2 value)
    {
        return _backward.ContainsKey(value);
    }

    public void Clear()
    {
        _forward.Clear();
        _backward.Clear();
    }
}

[Obsolete("Class is not integrated properly and is used as a template/reference")]
public class ComponentSparseSet<T> : IEnumerable<T> where T : IComponent
{
    private T[] _denseComponents;
    private EntityId[] _sparseIndices;
    private EntityId _entityCount;
    private int _capacity;

    public ComponentSparseSet(int capacity = 10)
    {
        _denseComponents = new T[capacity];
        _sparseIndices = new EntityId[capacity];
        _capacity = capacity;
        _entityCount = 0;
    }

    public void Add(EntityId id, T value)
    {
        _denseComponents[_entityCount] = value;
        _sparseIndices[id] = _entityCount;
        _entityCount++;
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}