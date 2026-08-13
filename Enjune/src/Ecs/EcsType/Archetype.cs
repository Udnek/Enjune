using Enjune.Misc;
using IComponent = Enjune.Ecs.Component.IComponent;

namespace Enjune.Ecs.EcsType;

public sealed class Archetype
{
    private readonly Dictionary<Type, IColumn> _columns = new();
    private EntityId[] _rowToId;
    private readonly Dictionary<EntityId, int> _idToRow;
    
    private int _capacity = EcsConstants.InitialCapacity;
    
    public int EntityCount { get; private set; } = 0;
    public Signature Signature { get; }

    public Archetype(Signature signature, World world)
    {
        Signature = signature;
        _rowToId = new EntityId[_capacity];
        _idToRow = new Dictionary<EntityId, int>(_capacity);
        
        int nComponents = signature.GetSetBitsCount();
        
        List<Type> types = world.ComponentManager.DeconstructSignature(signature);
        for (var i = 0; i < nComponents; i++) 
            RegisterColumn(types[i]);
    }

    public EntityId GetIdByRow(int row) => _rowToId[row];
    
    private void RegisterColumn(Type compType)
    {
        Type columnType = typeof(Column<>).MakeGenericType(compType);
        var columnInstance = Activator.CreateInstance(columnType, _capacity) as IColumn;
        _columns[compType] = columnInstance ?? 
                             throw new InvalidOperationException($"Failed to instantiate {Logger.GetTypeName(columnType)}");
    }

    private void EnsureCapacity(int targetCapacity)
    {
        if (targetCapacity <= _capacity) return;
        int newCapacity = _capacity * 2;
        
        Array.Resize(ref _rowToId, newCapacity);

        foreach (IColumn column in _columns.Values ) 
            column.SetCapacity(newCapacity);
        
        _capacity = newCapacity;
    }
    
    // TODO: Avoid using Collection<IComponent> because of boxing
    public void AddEntity(EntityAssembly entityAssembly, EntityId id)
    {
        Logger.Info(this, $"archetype with signature {Signature} acquired an entity {id}");
        
        EnsureCapacity(EntityCount + 1);

        int row = EntityCount;
        _idToRow[id] = row;
        _rowToId[row] = id;

        List<IComponent> entityComponents = entityAssembly.GetComponents();
        foreach (IComponent entityComponent in entityComponents)
        {
            if (_columns.ContainsKey(entityComponent.GetType())) 
                _columns[entityComponent.GetType()].SetValue(row, entityComponent);
        }
         
        EntityCount++;
    }

    
    public void RemoveEntity(EntityId id)
    {
        if (!_idToRow.TryGetValue(id, out var entityRow)) 
            Logger.Info(this, $"RemoveEntity: Entity {id} is not in {Signature} archetype.");
        
        var lastRow = EntityCount - 1;

        if (entityRow != lastRow)
        {
            var lastId = _rowToId[lastRow];

            foreach (IColumn column in _columns.Values) 
                column.SwapElements(lastRow, entityRow);

            _idToRow[lastId] = entityRow;
            _rowToId[entityRow] = lastId;
        }
        _idToRow.Remove(id);
        EntityCount--;
        Logger.Info(this, $"Removed entity {id} successfully");
    }

    public Span<T> GetComponents<T>() where T : struct, IComponent
    {
        Column<T> column = (Column<T>)_columns[typeof(T)];
        return column.GetSpan();
    }

    public bool ContainsEntity(EntityId id) => _idToRow.ContainsKey(id);

<<<<<<< Updated upstream
    // TODO: Entity Assembly is no longer suitable for runtime acquisation 
    [Obsolete]
    public EntityAssembly? GetAssembly(EntityId id)
=======
    public Entity.Snapshot GetSnapshot(EntityId id)
>>>>>>> Stashed changes
    {
        //if (!ContainsEntity(id)) return null;
        //int row = _id2Row[id];
        //EntityAssembly assembly = new EntityAssembly();
        //foreach (IColumn column in _columns.Values)
        //{
        //    assembly.AddComponent(column.GetValue(row));
        //}

        //return assembly;
        return null;
    }

    //TODO
    //public IEnumerable<Entity.Snapshot> GetAllEntities()
    //{
        
    //}
}