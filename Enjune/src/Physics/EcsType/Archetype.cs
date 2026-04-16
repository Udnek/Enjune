using System.ComponentModel;
using System.Globalization;
using Enjune.Physics.EcsType;
using FreeTypeSharp;
using IComponent = Enjune.Physics.Component.IComponent;

namespace Enjune.Physics.EcsType;

public class Archetype
{
    private Array[] _columns;
    private readonly Dictionary<Type, int> _component2Column = new();
    private EntityId[] _row2Id;
    private readonly Dictionary<EntityId, int> _id2Row;
    
    private readonly Signature _signature;
    
    private int _capacity = EcsConstants.InitialCapacity;
    private int _entityCount = 0;
    public int EntityCount => _entityCount;

    public Archetype(Signature signature)
    {
        _signature = signature;
        _row2Id = new EntityId[_capacity];
        _id2Row = new Dictionary<EntityId, int>(_capacity);
        
        int nComponents = signature.GetSetBitsCount();
        _columns = new Array[nComponents];
        
        List<Type> types = World.ComponentManager.DeconstructSignature(_signature);
        for (int i = nComponents - 1; i >= 0; i--)
        {
            _columns[i] = Array.CreateInstance(types[i], _capacity);
            _component2Column.Add(types[i], i);
        }
    }

    private void EnsureCapacity()
    {
        if (_entityCount + 1 <= _capacity) return;
        int newCapacity = _capacity * 2;
        
        Array.Resize(ref _row2Id, newCapacity);

        for (int i = 0; i < _columns.Length; i++)
        {
            var oldColumn = _columns[i];
            var newColumn = Array.CreateInstance(_columns[i].GetType().GetElementType()!, newCapacity);
            Array.Copy(oldColumn, newColumn, _entityCount);
            _columns[i] = newColumn;
        }
        
        _capacity = newCapacity;
    }
    
    public void AddEntity(EntityId id, IComponent[] components)
    {
        EnsureCapacity();
        int row = _entityCount;
        _id2Row[id] = row;
        _row2Id[row] = id;
        foreach (var component in components)
        {
            var componentType = component.GetType();
            int columnId = _component2Column[componentType];
            _columns[columnId].SetValue(component, row);
        }
        
        _entityCount++;
    }

    public void RemoveEntity(EntityId id)
    {
        if (!_id2Row.TryGetValue(id, out int row)) return;
        int lastRow = _entityCount - 1;
        if (row != lastRow)
        {
            EntityId lastId = _row2Id[lastRow];

            foreach (var column in _columns)
            {
                column.SetValue(column.GetValue(lastRow), row);
            }

            _id2Row[lastId] = row;
            _row2Id[row] = lastId;
        }
        
        _id2Row.Remove(id);
        _entityCount--;
    }

    public bool AssertSignature(Signature otherSignature) => _signature == otherSignature;
}

file class EntityToIdMap
{
    private readonly Dictionary<EntityId, int> _id2Row = new();
    private readonly Dictionary<int, EntityId> _row2Id = new();

    public void Set(EntityId id,  int row)
    {
        _id2Row[id] = row;
        _row2Id[row] = id;
    }

    public void RemoveByEntity(EntityId id)
    {
        _row2Id.Remove(_row2Id[id]);
        _row2Id.Remove(id);
    }

    public int GetRow(EntityId id)
    {
        return _id2Row[id];
    }

    public EntityId GetId(EntityId id)
    {
        return _row2Id[id];
    }

    public bool ContainsEntity(EntityId id)
    {
        return _id2Row.ContainsKey(id);
    }

    public bool ContainsRow(int row)
    {
        return _row2Id.ContainsKey(row);
    }
}

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