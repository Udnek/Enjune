using Enjune.Attribute;
using Enjune.Data;
using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.Manager;
using Enjune.Ecs.System;
using Enjune.Misc;
using FreeTypeSharp;

namespace Enjune.Ecs;

[LogParams(logCallingMethod: true)]
public sealed class World
{
    public static readonly ICodec<World> WithoutSystemsCodec = new SimpleCodec<World>(
        world =>
        {
            var allEntities = world.GetAllEntities().ToList();
            List<DataObject> encodedEntities = new(allEntities.Count);
            foreach (var (entity, components) in allEntities)
            {
                var result = IComponent.ArrayCodec.Encode(components.ToArray());
                if (result.Error != null)
                    return new Error($"can not encode {entity}'s components: {result.Error}");
                
                encodedEntities.Add(result.GetOrThrow());
            }

            return ResultOrError.Success<DataObject>(encodedEntities.ToArray());
        },
        data =>
        {
            var array = data.Cast<DataObject.Array>(out var castErr);
            if (array is null)
                return new Error($"can not decode: {castErr}");

            List<Entity.Assembly> entities = [];
            foreach (var compsData in array.Val)
            {
                var result = IComponent.ArrayCodec.Decode(compsData);
                if (result.Error != null)
                    return new Error($"can not decode components: {result.Error}");
                var assembly = new Entity.Assembly();
                foreach (var comp in result.GetOrThrow()) 
                    assembly.AddComponent(comp);
                entities.Add(assembly);
            }

            var world = new World();
            foreach (var assembly in entities) 
                world.AddEntity(assembly);
            return ResultOrError.Success(world);
        });
    
    internal readonly ArchetypeManager ArchetypeManager;
    internal readonly SystemManager SystemManager;
    internal readonly ComponentManager ComponentManager = new ComponentManager();
    internal readonly EntityManager EntityManager = new EntityManager();

    // This cache version marks broad archetype structure version:
    // it increments when a new archetype gets created, but does not
    // increment when archetype's entity container changes
    //private int _cacheVersion = 0;
    //internal int CacheVersion { get => ++_cacheVersion; private set => _cacheVersion = value; }
    internal int CacheVersion { get; private set; }
    private readonly List<Entity> _entities = [];

    // TODO: Huge placeholder, need to come up with something better
    private List<Entity> _entityRemoveQueue = [];
    private List<(Entity, Type)> _componentRemoveQueue = [];
    private List<(Entity, IComponent, Type)> _componentAddQueue = [];
    private bool _locked = false;
    
    public World()
    {
        Logger.Info(this, "Registering managers");

        ArchetypeManager = new ArchetypeManager(this);
        SystemManager = new SystemManager(this);
    }
    
    /// <summary>
    /// Increments world cache version
    /// </summary>
    private void InvalidateCache()
    {
        Logger.Info(this, "Invalidated cache");
        CacheVersion++;
    }

    /// <summary>
    /// Locks the world, deferring certain actions that would
    /// invalidate cache until the world is unlocked. 
    /// </summary>
    internal void Lock()
    {
        Logger.Info(this, "Locked");
        _locked = true;
    }

    /// <summary>
    /// Unlocks the world, executing all deferred actions and invalidating world cache
    /// </summary>
    internal void Unlock()
    {
        Logger.Info(this, "Unlocked");
        if (!_locked) return;
        Logger.Info(this, "Purging commands");
        _locked = false;
        InvalidateCache();
        foreach (var entity in _entityRemoveQueue)
        {
            RemoveEntity(entity);
        }
        _entityRemoveQueue.Clear();
        foreach (var (entity, componentType) in _componentRemoveQueue)
        {
            RemoveEntityComponent(entity, componentType);
        }
        _componentRemoveQueue.Clear();
        foreach (var (entity, component, componentType) in _componentAddQueue)
        {
            AddEntityComponent(entity, component, componentType);
        }
        _componentAddQueue.Clear();
    }

    private int GetComponentId(Type component) => ComponentManager.GetIdByType(component);
    
    internal IEnumerable<Archetype> QueryArchetypes(Signature include, Signature exclude)
        => ArchetypeManager.Query(include, exclude);

    #region Public Api

    public void AddSystem(ISystem system) => SystemManager.RegisterSystem(system);

    public void Update() => SystemManager.UpdateAll();

    #region Entity Interactions
    public Entity AddEntity(Entity.Assembly assembly)
    {
        Entity entity = EntityManager.CreateEntity();
        ArchetypeManager.AddEntity(assembly, entity);
        _entities.Add(entity);
        InvalidateCache();
        return entity;
    }

    /// <summary>
    /// Attempts to remove an entity.<br/>
    /// Does nothing if there is no such entity.<br/>
    /// If the world is locked, defers operation until it's unlocked.
    /// </summary>
    /// <param name="entity">Entity.</param>
    public void RemoveEntity(Entity entity)
    {
        if (_locked)
        {
            Logger.Warn(this, $"Deferred removal of {entity}");
            _entityRemoveQueue.Add(entity);
            return;
        }
        ArchetypeManager.RemoveEntity(entity);
        _entities.Remove(entity);
    }

    // TODO: Cringe ass idk son im crine
    /// <summary>
    /// Attempts to add a component from an entity.<br/>
    /// Errors if an entity already has such component.<br/>
    /// If the world is locked, defers operation until it's unlocked.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <param name="component">The component to add.</param>
    /// <param name="componentType">The component type.</param>
    public void AddEntityComponent(Entity entity, IComponent component, Type componentType)
    {
        if (_locked)
        {
            _componentAddQueue.Add((entity, component, componentType));
            return;
        }
        if (!_entities.Contains(entity))
        {
            Logger.Error(this, $"{entity} doesn't exist");
            return;
        }
        Archetype currentArchetype = ArchetypeManager.GetArchetypeByEntity(entity);
        Signature targetSignature = currentArchetype.Signature.Set(GetComponentId(componentType));

        if (targetSignature.Equals(currentArchetype.Signature))
        {
            Logger.Error(this, $"{entity} already has {componentType}. Use {nameof(ModifyEntityComponent)}");
            return;
        }

        Archetype targetArchetype = ArchetypeManager.GetOrAddArchetypeBySignature(targetSignature);

        ArchetypeManager.MoveEntity(entity, currentArchetype, targetArchetype);
        targetArchetype.SetComponent(entity, component);

        Logger.Info(this, $"Added {componentType} to {entity}");
        return;
    }
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <inheritdoc cref="AddEntityComponent(Entity, IComponent, Type)"/>
    public void AddEntityComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct, IComponent
    {
        AddEntityComponent(entity, component, typeof(TComponent));
    }

    /// <summary>
    /// Attempts to remove a component from an entity.<br/>
    /// Does nothing if an entity doesn't have specified component.<br/>
    /// If the world is locked, defers operation until it's unlocked.
    /// </summary>
    /// <param name="entity">Entity.</param>
    /// <param name="componentType">The component type to remove.</param>
    public void RemoveEntityComponent(Entity entity, Type componentType)
    {
        // TODO: Move removal execution into a separate method to avoid an unnecessary check?
        if (_locked)
        {
            Logger.Warn(this, $"Deferred removal of {componentType} from {entity}");
            _componentRemoveQueue.Add((entity, componentType));
            return;
        }
        if (!_entities.Contains(entity))
        {
            Logger.Error(this, $"{entity} doesn't exist");
            return;
        }
        Archetype currentArchetype = ArchetypeManager.GetArchetypeByEntity(entity);
        Signature targetSignature = currentArchetype.Signature.Unset(GetComponentId(componentType));

        if (targetSignature.Equals(currentArchetype.Signature))
        {
            Logger.Error(this, $"{entity} doesn't have {componentType}");
            return;
        }

        Archetype targetArchetype = ArchetypeManager.GetOrAddArchetypeBySignature(targetSignature);

        ArchetypeManager.MoveEntity(entity, currentArchetype, targetArchetype);

        Logger.Info(this, $"Removed {componentType} from {entity} successfully");
    }
    /// <typeparam name="TComponent">The component type to remove.</typeparam>
    /// <inheritdoc cref="RemoveEntityComponent(Entity, Type)"/>
    public void RemoveEntityComponent<TComponent>(Entity entity) where TComponent : struct, IComponent
    {
        RemoveEntityComponent(entity, typeof(TComponent));
    }

    #endregion

    #region Heavy Api

    // Don't use in hot loops
    // Returns a copy of a component
    public TComponent? GetEntityComponent<TComponent>(Entity entity) where TComponent : struct, IComponent
    {
        if (!_entities.Contains(entity))
        {
            Logger.Error(this, $"{entity} doesn't exist");
            return null;
        }

        Archetype archetype = ArchetypeManager.GetArchetypeByEntity(entity);
        if (!archetype.Signature.IsSet(GetComponentId(typeof(TComponent))))
        {
            Logger.Info(this, $"{entity} doesn't have {typeof(TComponent)}");
            return null;
        }

        return archetype.GetComponent<TComponent>(entity);
    }

    // Don't use in hot loops
    public bool ModifyEntityComponent<TComponent>(Entity entity, Func<TComponent, TComponent> modifier) where TComponent : struct, IComponent
    {
        if (!_entities.Contains(entity))
        {
            Logger.Error(this, $"{entity} doesn't exist");
            return false;
        }

        Archetype archetype = ArchetypeManager.GetArchetypeByEntity(entity);
        archetype.ModifyComponent(entity, modifier);
        return true;
    }

    // Don't use in hot loops
    public IEnumerable<IComponent> GetEntityComponents(Entity entity)
    {
        return ArchetypeManager.GetArchetypeByEntity(entity).GetEntityComponents(entity);
    }
    
    // Don't use in hot loops
    public IEnumerable<(Entity Entity, List<IComponent> Components)> GetAllEntities()
    {
        foreach (var archetype in QueryArchetypes(Signature.Empty, Signature.Empty))
        {
            foreach (var snapshot in archetype.GetEntitySnapshots())
            {
                yield return snapshot;
            }
        }
    }

    #endregion

    #endregion
}