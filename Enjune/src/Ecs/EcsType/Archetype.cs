using Enjune.Misc;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using IComponent = Enjune.Ecs.Component.IComponent;

namespace Enjune.Ecs.EcsType;

public sealed class Archetype
{
    public int Rows { get; private set; } = 0;
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

    #region Public Api

    public Entity GetEntityByRow(int row) => _rowToEntity[row];

    public Span<T> GetComponents<T>() where T : struct, IComponent
    {
        Column<T> column = (Column<T>)_columns[typeof(T)];
        return column.GetSpan();
    }

    #endregion

    internal bool Contains(Entity entity) => _rowToEntity.Contains(entity);

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
    internal void AddEntity(Entity.Assembly entityAssembly, Entity entity)
    {
        Logger.Info(Logger.Domain.Ecs, $"{this}[{Signature}].{nameof(AddEntity)}", $"Acquired {entity} as an assembly");
        
        EnsureCapacity(Rows + 1);

        int row = Rows;
        _entityToRow[entity] = row;
        _rowToEntity[row] = entity;
        
        foreach (IComponent component in entityAssembly.GetComponents())
        {
            if (_columns.ContainsKey(component.GetType())) 
                _columns[component.GetType()].SetValue(row, component);
        }
         
        Rows++;
    }
    
    internal void RemoveEntity(Entity entity)
    {
        if (!_entityToRow.TryGetValue(entity, out var entityRow)) 
            Logger.Info(Logger.Domain.Ecs, $"{this}[{Signature}].{nameof(RemoveEntity)}", $"{entity} is not in {Signature} archetype.");
        Logger.Info(Logger.Domain.Ecs, $"{this}[{Signature}].{nameof(RemoveEntity)}", $"Removing {entity}");

        var lastRow = Rows - 1;

        if (entityRow != lastRow)
        {
            var lastId = _rowToEntity[lastRow];

            foreach (IColumn column in _columns.Values) 
                column.SwapElements(lastRow, entityRow);

            _entityToRow[lastId] = entityRow;
            _rowToEntity[entityRow] = lastId;
        }
        _entityToRow.Remove(entity);
        Rows--;
    }
    
    private (Entity, List<IComponent>)? GetSnapshot(Entity entity)
    {
        if (!_rowToEntity.Contains(entity))
        {
            Logger.Warn(Logger.Domain.Ecs, $"{this}[{Signature}].{nameof(GetSnapshot)}", $"Requested {entity} doesn't exist in this archetype");
            return null;
        }

        List<IComponent> components = [];
        foreach (IColumn column in _columns.Values)
        {
            components.Add(column.GetValue(_entityToRow[entity]));
        }
        return (entity, components);
    }
    
    internal IEnumerable<(Entity, List<IComponent>)> GetAllEntitySnapshots()
    {
        foreach (var entity in _rowToEntity)
        {
            yield return GetSnapshot(entity)!.Value;
        }
    }

    internal IEnumerable<IComponent> GetAllEntityComponents(Entity entity)
    {
        var index = _entityToRow[entity];
        foreach ((Type type, IColumn column) in _columns)
        {
            yield return column.GetValue(index);
        }
    }

    internal void AddEntity(Entity entity, IEnumerable<IComponent> components)
    {
        Logger.Info(Logger.Domain.Ecs, $"{this}[{Signature}].{nameof(AddEntity)}", $"Acquired {entity} as a stream of components");

        EnsureCapacity(Rows + 1);

        int row = Rows;
        _entityToRow[entity] = row;
        _rowToEntity[row] = entity;

        foreach (IComponent component in components)
        {
            if (_columns.ContainsKey(component.GetType()))
            {
                _columns[component.GetType()].SetValue(row, component);
            } else {
                Logger.Info(Logger.Domain.Ecs, $"{this}[{Signature}].{nameof(AddEntity)}", $"Omitting a component that does not belong to archetype {Signature}");
            }
        }

        Rows++;
    }

    internal TComponent GetComponentCopy<TComponent>(Entity entity) where TComponent : struct, IComponent
    {
        int row = _entityToRow[entity];
        return ((Column<TComponent>)_columns[typeof(TComponent)])[row];
    }

    internal void ModifyComponent<TComponent>(Entity entity, Func<TComponent, TComponent> modification) where TComponent: struct, IComponent
    {
        int row = _entityToRow[entity];
        Column<TComponent> column = (Column<TComponent>)_columns[typeof(TComponent)];
        column[row] = modification(column[row]);
    }

    internal void WriteComponent(Entity entity, IComponent component)
    {
        int row = _entityToRow[entity];
        var column = _columns[component.GetType()];
        column.SetValue(row, component);
    }
}