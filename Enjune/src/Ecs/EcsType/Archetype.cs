using Enjune.Misc;
using IComponent = Enjune.Ecs.Component.IComponent;

namespace Enjune.Ecs.EcsType;

public sealed class Archetype
{
    private readonly Dictionary<Type, IColumn> _columns = new();
    private EntityId[] _row2Id;
    private readonly Dictionary<EntityId, int> _id2Row;
    
    private int _capacity = EcsConstants.InitialCapacity;
    
    public int EntityCount { get; private set; } = 0;
    public Signature Signature { get; }

    public Archetype(Signature signature, World world)
    {
        Signature = signature;
        _row2Id = new EntityId[_capacity];
        _id2Row = new Dictionary<EntityId, int>(_capacity);
        
        int nComponents = signature.GetSetBitsCount();
        
        List<Type> types = world.ComponentManager.DeconstructSignature(signature);
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
        Logger.Log(this, $"archetype with signature {Signature} acquired an entity {entityAssembly.Id}");

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
        if (!_id2Row.TryGetValue(id, out var entityRow))
        {
            Logger.Log(this, $"RemoveEntity: Entity {id} is not in {Signature} archetype.");
        }
        var lastRow = EntityCount - 1;

        if (entityRow != lastRow)
        {
            var lastId = _row2Id[lastRow];

            foreach (IColumn column in _columns.Values)
            {
                column.SwapElements(lastRow, entityRow);
            }

            _id2Row[lastId] = entityRow;
            _row2Id[entityRow] = lastId;
        }
        _id2Row.Remove(id);
        EntityCount--;
        Logger.Log(this, $"Removed entity {id} successfully");
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