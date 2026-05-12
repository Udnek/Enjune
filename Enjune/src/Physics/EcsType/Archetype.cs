using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Enjune.Misc;
using Enjune.Physics.EcsType;
using FreeTypeSharp;
using IComponent = Enjune.Physics.Component.IComponent;

namespace Enjune.Physics.EcsType;

public class Archetype
{
    private readonly Dictionary<Type, IColumn> _columns = new();
    private EntityId[] _row2Id;
    private readonly Dictionary<EntityId, int> _id2Row;
    
    private int _capacity = EcsConstants.InitialCapacity;
    
    public int EntityCount { get; private set; } = 0;
    public Signature Signature { get; }

    public Archetype(Signature signature)
    {
        Signature = signature;
        _row2Id = new EntityId[_capacity];
        _id2Row = new Dictionary<EntityId, int>(_capacity);
        
        int nComponents = signature.GetSetBitsCount();
        
        List<Type> types = World.ComponentManager.DeconstructSignature(signature);
        for (var i = 0; i < nComponents; i++)
        {
            RegisterColumn(types[i]);
        }
    }

    public EntityId GetIdByRow(int row) => _row2Id[row];
    
    private void RegisterColumn(Type type)
    {
        Type columnType = typeof(Column<>).MakeGenericType(type);
        var columnInstance = Activator.CreateInstance(columnType, _capacity) as IColumn;
        _columns[type] = columnInstance ?? throw new InvalidOperationException($"Failed to instantiate Column<{type.Name}>");
    }

    private void EnsureCapacity()
    {
        if (EntityCount + 1 <= _capacity) return;
        int newCapacity = _capacity * 2;
        
        Array.Resize(ref _row2Id, newCapacity);

        foreach (IColumn column in _columns.Values )
        {
            column.SetCapacity(newCapacity);
        }
        
        _capacity = newCapacity;
    }
    
    // TODO: Avoid using Collection<IComponent> because of boxing
    public void AddEntity(EntityAssembly entityAssembly)
    {
        Logger.Log(GetType(), $"archetype with signature {Signature} acquired an entity {entityAssembly.Id}");
        EnsureCapacity();
        int row = EntityCount;
        _id2Row[entityAssembly.Id] = row;
        _row2Id[row] = entityAssembly.Id;
        List<IComponent> entityComponents = entityAssembly.GetComponents();
        foreach (IComponent entityComponent in entityComponents)
        {
            if (_columns.ContainsKey(entityComponent.GetType()))
            {
                _columns[entityComponent.GetType()].SetValue(row, entityComponent);
            }
        }
         
        EntityCount++;
    }

    public void RemoveEntity(EntityId id)
    {
        if (!_id2Row.TryGetValue(id, out int row)) return;
        int lastRow = EntityCount - 1;
        if (row != lastRow)
        {
            EntityId lastId = _row2Id[lastRow];

            foreach (IColumn column in _columns.Values)
            {
                column.SwapElements(lastRow, row);
            }

            _id2Row[lastId] = row;
            _row2Id[row] = lastId;
        }
        
        _id2Row.Remove(id);
        EntityCount--;
    }

    public Span<T> GetComponents<T>() where T : struct, IComponent
    {
        Column<T> column = (Column<T>)_columns[typeof(T)];
        return column.GetSpan();
    }

    public bool ContainsEntity(EntityId id)
    {
        return _id2Row.ContainsKey(id);
    }

    public EntityAssembly? GetAssembly(EntityId id)
    {
        if (!ContainsEntity(id)) return null;
        int row = _id2Row[id];
        EntityAssembly assembly = new EntityAssembly(id);
        foreach (IColumn column in _columns.Values)
        {
            assembly.AddComponent(column.GetValue(row));
        }

        return assembly;
    }
}

[Obsolete("Unfinished, may be unusable")]
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