using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
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
    private int _entityCount = 0;
    public int EntityCount => _entityCount;

    public Archetype(Signature signature)
    {
        _row2Id = new EntityId[_capacity];
        _id2Row = new Dictionary<EntityId, int>(_capacity);
        
        int nComponents = signature.GetSetBitsCount();
        
        List<Type> types = World.ComponentManager.DeconstructSignature(signature);
        for (int i = nComponents - 1; i >= 0; i--)
        {
            RegisterColumn(types[i]);
        }
    }

    private void RegisterColumn(Type type)
    {
        Type columnType = typeof(Column<>).MakeGenericType(type);
        var columnInstance = Activator.CreateInstance(columnType, _capacity) as IColumn;
        _columns[type] = columnInstance ?? throw new InvalidOperationException($"Failed to instantiate Column<{type.Name}>");
    }

    private void EnsureCapacity()
    {
        if (_entityCount + 1 <= _capacity) return;
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
        EnsureCapacity();
        int row = _entityCount;
        _id2Row[entityAssembly.Id] = row;
        _row2Id[row] = entityAssembly.Id;
        List<IComponent> components = entityAssembly.GetComponents();
        foreach (IComponent component in components)
        {
            _columns[component.GetType()].SetValue(row, component);
        }
        
        _entityCount++;
    }

    [Obsolete("Not implemented with the new system")]
    public void RemoveEntity(EntityId id)
    {
        if (!_id2Row.TryGetValue(id, out int row)) return;
        int lastRow = _entityCount - 1;
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
        _entityCount--;
    }

    public Span<T> GetComponents<T>() where T : struct, IComponent
    {
        Column<T> column = (Column<T>)_columns[typeof(T)];
        return column.GetSpan();
    }
}