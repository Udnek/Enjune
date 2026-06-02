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

    [Obsolete("Not implemented with the new system")]
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