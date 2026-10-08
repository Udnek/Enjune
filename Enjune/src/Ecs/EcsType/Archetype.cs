using Enjune.Attribute;
using Enjune.Misc;
using IComponent = Enjune.Ecs.Component.IComponent;

namespace Enjune.Ecs.EcsType;

[LogParams(logCallingMethod: true, method: LogParamsAttribute.Method.ToString)]
public sealed class Archetype
{
    public int Count { get; private set; } = 0;
    public readonly Signature Signature;
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
    
    internal void AddEntity(Entity.Assembly entityAssembly, Entity entity)
    {
        Logger.Info(this, $"Acquired {entity} as an assembly");
        
        EnsureCapacity(Count + 1);

        int row = Count;
        _entityToRow[entity] = row;
        _rowToEntity[row] = entity;
        
        foreach (IComponent component in entityAssembly.GetComponents())
        {
            if (_columns.ContainsKey(component.GetType()))
            {
                var column = _columns[component.GetType()];
                column.SetValue(row, component);
                column.Count++;
            }
        }

        Count++;
    }

    internal void AddEntity(Entity entity, IEnumerable<IComponent> components)
    {
        Logger.Info(this, $"Acquired {entity} as a stream of components");

        EnsureCapacity(Count + 1);

        int row = Count;
        _entityToRow[entity] = row;
        _rowToEntity[row] = entity;

        foreach (IComponent component in components)
        {
            if (_columns.ContainsKey(component.GetType()))
            {
                var column = _columns[component.GetType()];
                column.SetValue(row, component);
                column.Count++;
            }
            else
            {
                Logger.Info(this, $"Omitting a component that does not belong to archetype {Signature}");
            }
        }

        Count++;
    }

    internal void RemoveEntity(Entity entity)
    {
        if (!_entityToRow.TryGetValue(entity, out var entityRow)) 
            Logger.Info(this, $"{entity} is not in {Signature} archetype.");
        Logger.Info(this, $"Removing {entity}");

        var lastRow = Count - 1;

        if (entityRow != lastRow)
        {
            var lastId = _rowToEntity[lastRow];

            foreach (IColumn column in _columns.Values)
            {
                column.SwapElements(lastRow, entityRow);
                column.Count--;
            }

            _entityToRow[lastId] = entityRow;
            _rowToEntity[entityRow] = lastId;
        }
        else
        {
            foreach (IColumn column in _columns.Values)
            {
                column.Count--;
            }
        }

        _entityToRow.Remove(entity);
        Count--;
    }
    
    internal IEnumerable<IComponent> GetEntityComponents(Entity entity)
    {
        var index = _entityToRow[entity];
        foreach ((Type _, IColumn column) in _columns)
            yield return column.GetValue(index);
    }
    
    internal IEnumerable<(Entity, List<IComponent>)> GetEntitySnapshots()
    {
        for (int row = 0; row < Count; row++)
            yield return (_rowToEntity[row], GetEntityComponents(_rowToEntity[row]).ToList());
    }

    internal TComponent GetComponent<TComponent>(Entity entity) where TComponent : struct, IComponent
    {
        int row = _entityToRow[entity];
        return ((Column<TComponent>)_columns[typeof(TComponent)])[row];
    }

    // Modifies a component effectively using a delegate
    internal void ModifyComponent<TComponent>(Entity entity, Func<TComponent, TComponent> modifier) where TComponent: struct, IComponent
    {
        int row = _entityToRow[entity];
        Column<TComponent> column = (Column<TComponent>)_columns[typeof(TComponent)];
        column[row] = modifier(column[row]);
    }

    // Completely rewrites a component by copying a new one in place of the old one
    internal void SetComponent(Entity entity, IComponent component)
    {
        int row = _entityToRow[entity];
        var column = _columns[component.GetType()];
        column.SetValue(row, component);
    }

    internal Column<TComponent> GetColumn<TComponent>() where TComponent : struct, IComponent
    {
        return (Column<TComponent>)_columns[typeof(TComponent)];
    }

    internal Entity[] GetEntities() => _rowToEntity;
    
    public override string ToString() => $"{nameof(Archetype)}[{Signature}]";
}