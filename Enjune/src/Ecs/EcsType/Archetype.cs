using Enjune.Misc;
using IComponent = Enjune.Ecs.Component.IComponent;

namespace Enjune.Ecs.EcsType;

public sealed class Archetype
{
    public int EntityCount { get; private set; } = 0;
    public Signature Signature { get; }
    
    private readonly Dictionary<Type, IColumn> _columns = new();
    private Entity[] _rowToEntity;
    private readonly Dictionary<Entity, int> _entityToRow;
    private int _capacity = EcsConstants.InitialColumnCapacity;

    public Archetype(Signature signature, World world)
    {
        Signature = signature;
        _rowToEntity = new Entity[_capacity];
        _entityToRow = new Dictionary<Entity, int>(_capacity);
        
        int nComponents = signature.GetSetBitsCount();
        
        List<Type> types = world.ComponentManager.DeconstructSignature(signature);
        for (var i = 0; i < nComponents; i++) 
            RegisterColumn(types[i]);
    }

    public Entity GetEntityByRow(int row) => _rowToEntity[row];
    
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
        
        Array.Resize(ref _rowToEntity, newCapacity);

        foreach (IColumn column in _columns.Values ) 
            column.SetCapacity(newCapacity);
        
        _capacity = newCapacity;
    }
    
    // TODO: Avoid using Collection<IComponent> because of boxing
    public void AddEntity(EntityAssembly entityAssembly, Entity entity)
    {
        Logger.Info(this, $"archetype with signature {Signature} acquired an entity {entity}");
        
        EnsureCapacity(EntityCount + 1);

        int row = EntityCount;
        _entityToRow[entity] = row;
        _rowToEntity[row] = entity;

        List<IComponent> entityComponents = entityAssembly.GetComponents();
        foreach (IComponent entityComponent in entityComponents)
        {
            if (_columns.ContainsKey(entityComponent.GetType())) 
                _columns[entityComponent.GetType()].SetValue(row, entityComponent);
        }
         
        EntityCount++;
    }

    
    public void RemoveEntity(Entity id)
    {
        if (!_entityToRow.TryGetValue(id, out var entityRow)) 
            Logger.Info(this, $"RemoveEntity: Entity {id} is not in {Signature} archetype.");
        
        var lastRow = EntityCount - 1;

        if (entityRow != lastRow)
        {
            var lastId = _rowToEntity[lastRow];

            foreach (IColumn column in _columns.Values) 
                column.SwapElements(lastRow, entityRow);

            _entityToRow[lastId] = entityRow;
            _rowToEntity[entityRow] = lastId;
        }
        _entityToRow.Remove(id);
        EntityCount--;
        Logger.Info(this, $"Removed entity {id} successfully");
    }

    public Span<T> GetComponents<T>() where T : struct, IComponent
    {
        Column<T> column = (Column<T>)_columns[typeof(T)];
        return column.GetSpan();
    }

    public bool ContainsEntity(Entity entity) => _entityToRow.ContainsKey(entity);
    public Entity.Snapshot? GetSnapshot(Entity entity)
    {
        if (!_rowToEntity.Contains(entity))
        {
            Logger.Warn(this, $"Snapshot: Requested entity {entity} doesn't exist in this archetype");
            return null;
        }

        Entity.Snapshot snapshot = new Entity.Snapshot(entity);
        foreach (IColumn column in _columns.Values)
        {
            snapshot.AddComponent(column.GetValue(_entityToRow[entity]));
        }
        return snapshot;
    }
    public IEnumerable<Entity.Snapshot> GetAllEntities()
    {
        foreach (var id in _rowToEntity)
        {
            yield return GetSnapshot(id)!;
        }
    }

    public Column<T> GetColumn<T>() where T : struct, IComponent
    {
        return (Column<T>)_columns[typeof(T)];
    }
}