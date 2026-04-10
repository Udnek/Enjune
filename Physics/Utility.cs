using System.Collections;
using System.ComponentModel;

namespace Enjune.Physics;

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

/// <summary>
/// A sparse set data structure that stores struct values indexed by integer keys.
/// Provides O(1) add, remove, contains, and iteration operations.
/// </summary>
/// <typeparam name="T">The type of elements stored. Must be a struct.</typeparam>
public class CopySparseSet<T> : IEnumerable<T> where T : struct
{
    private readonly Func<T, int> _keySelector;
    private int[] _sparse;           // maps key -> dense index (or -1 if not present)
    private int[] _denseKeys;        // keys stored in dense order
    private T[] _denseValues;        // values stored in dense order (parallel to _denseKeys)
    private int _count;              // number of elements currently in the set

    /// <summary>Gets the number of elements in the set.</summary>
    public int Count => _count;

    /// <summary>
    /// Initializes a new sparse set.
    /// </summary>
    /// <param name="keySelector">Function that extracts the integer key from a value.</param>
    /// <param name="initialDenseCapacity">Initial capacity of the dense arrays.</param>
    /// <param name="initialSparseCapacity">Initial size of the sparse array (max key + 1).</param>
    public CopySparseSet(Func<T, int> keySelector, int initialDenseCapacity = 64, int initialSparseCapacity = 1024)
    {
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        _sparse = new int[initialSparseCapacity];
        Array.Fill(_sparse, -1);              // -1 indicates "not present"
        _denseKeys = new int[initialDenseCapacity];
        _denseValues = new T[initialDenseCapacity];
        _count = 0;
    }

    /// <summary>Adds a value to the set. Throws if the key already exists.</summary>
    public void Add(T value)
    {
        int key = _keySelector(value);
        EnsureSparseCapacity(key);
        
        ref int denseIndex = ref _sparse[key];
        if (denseIndex != -1)
            throw new InvalidOperationException($"Key {key} already exists in the set.");

        // Add to dense arrays
        if (_count == _denseKeys.Length)
            ResizeDense();
        
        denseIndex = _count;
        _denseKeys[_count] = key;
        _denseValues[_count] = value;
        _count++;
    }

    /// <summary>Adds or updates a value based on its key.</summary>
    public void Set(T value)
    {
        int key = _keySelector(value);
        EnsureSparseCapacity(key);
        
        ref int denseIndex = ref _sparse[key];
        if (denseIndex != -1)
        {
            // Update existing value
            _denseValues[denseIndex] = value;
            return;
        }
        
        // Add new
        if (_count == _denseKeys.Length)
            ResizeDense();
        
        denseIndex = _count;
        _denseKeys[_count] = key;
        _denseValues[_count] = value;
        _count++;
    }

    /// <summary>Removes the value with the given key. Returns true if found.</summary>
    public bool RemoveByKey(int key)
    {
        if (key < 0) return false;
        if (key >= _sparse.Length) return false;
        
        int denseIndex = _sparse[key];
        if (denseIndex == -1) return false;
        
        // Swap with the last element
        int lastIndex = _count - 1;
        if (denseIndex != lastIndex)
        {
            int lastKey = _denseKeys[lastIndex];
            T lastValue = _denseValues[lastIndex];
            
            _denseKeys[denseIndex] = lastKey;
            _denseValues[denseIndex] = lastValue;
            _sparse[lastKey] = denseIndex;
        }
        
        // Remove last element
        _sparse[key] = -1;
        _count--;
        return true;
    }

    /// <summary>Removes a value from the set (using its key). Returns true if found.</summary>
    public bool Remove(T value) => RemoveByKey(_keySelector(value));

    /// <summary>Checks whether a key exists in the set.</summary>
    public bool ContainsKey(int key)
    {
        return key >= 0 && key < _sparse.Length && _sparse[key] != -1;
    }

    /// <summary>Checks whether a value exists in the set (by extracting its key).</summary>
    public bool Contains(T value) => ContainsKey(_keySelector(value));

    /// <summary>Gets the value associated with the specified key.</summary>
    public bool TryGetValue(int key, out T value)
    {
        if (key >= 0 && key < _sparse.Length)
        {
            int denseIndex = _sparse[key];
            if (denseIndex != -1)
            {
                value = _denseValues[denseIndex];
                return true;
            }
        }
        value = default;
        return false;
    }

    /// <summary>Indexer to get or set a value by its key.</summary>
    public T this[int key]
    {
        get
        {
            if (TryGetValue(key, out T value))
                return value;
            throw new KeyNotFoundException($"Key {key} not found.");
        }
        set
        {
            int k = key;
            EnsureSparseCapacity(k);
            ref int denseIndex = ref _sparse[k];
            if (denseIndex != -1)
            {
                _denseValues[denseIndex] = value;
            }
            else
            {
                if (_count == _denseKeys.Length)
                    ResizeDense();
                denseIndex = _count;
                _denseKeys[_count] = k;
                _denseValues[_count] = value;
                _count++;
            }
        }
    }

    /// <summary>Removes all elements from the set.</summary>
    public void Clear()
    {
        // Reset dense count and mark sparse entries as -1 only for keys that were used
        for (int i = 0; i < _count; i++)
        {
            _sparse[_denseKeys[i]] = -1;
        }
        _count = 0;
    }

    /// <summary>Returns an enumerator that iterates through the values in the set.</summary>
    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
            yield return _denseValues[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // --------------------------------------------------------------------------
    // Private helpers
    // --------------------------------------------------------------------------
    private void EnsureSparseCapacity(int key)
    {
        if (key >= _sparse.Length)
        {
            int newSize = Math.Max(key + 1, _sparse.Length * 2);
            Array.Resize(ref _sparse, newSize);
            // Initialize new entries to -1
            for (int i = _sparse.Length / 2; i < newSize; i++)
                _sparse[i] = -1;
        }
    }

    private void ResizeDense()
    {
        int newCapacity = _denseKeys.Length == 0 ? 4 : _denseKeys.Length * 2;
        Array.Resize(ref _denseKeys, newCapacity);
        Array.Resize(ref _denseValues, newCapacity);
    }
}