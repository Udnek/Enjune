using System.Runtime.CompilerServices;
using Enjune.Data;
using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.Manager;
using Enjune.Ecs.System;
using Enjune.Logger;
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

    /// <summary>
    /// Internal structure version.
    /// </summary>
    internal int CacheVersion { get; private set; }
    private readonly List<Entity> _entities = [];

    #region Locking To Prevent Strcture Change During Query Iteration

    // TODO: Huge placeholder, need to come up with something better
    private readonly List<Entity> _entityRemoveQueue = [];
    private readonly List<(Entity, Type)> _componentRemoveQueue = [];
    private readonly List<(Entity, IComponent, Type)> _componentAddQueue = [];
    private int _locks = 0;
    /// <summary>
    /// <para>
    /// If internal lock counter is greater than <c>0</c>, then this value is <c>true</c>, and while it is,<br/>
    /// <see cref="World"/> can't accept structural changes, including but potentially not limited to:
    /// <list type="bullet">
    /// <item><see cref="AddEntity(Entity.Assembly)"/></item>
    /// <item><see cref="RemoveEntity(Entity)"/></item>
    /// <item><see cref="RemoveEntityComponent(Entity, Type)"/></item>
    /// </list>
    /// </para>
    /// </summary>
    private bool IsLocked => _locks > 0;

    #endregion
    
    public World()
    {
        Logger.Logger.Info(this, "New World instance created");
        Logger.Logger.Info(this, "Registering managers");

        ArchetypeManager = new ArchetypeManager(this);
        SystemManager = new SystemManager(this);
    }
    
    /// <summary>
    /// Increments world cache version
    /// </summary>
    private void InvalidateCache()
    {
        Logger.Logger.Info(this, "Invalidated cache");
        CacheVersion++;
    }

    /// <summary>
    /// Increments internal lock counter by 1. See <see cref="IsLocked"/>
    /// </summary>
    internal void Lock() => _locks += 1;

    /// <summary>
    /// <para>
    /// Decrements internal lock counter by 1. See <see cref="IsLocked"/>
    /// </para>
    /// <para>
    /// If <see cref="IsLocked"/> is <c>false</c> after decrementing,
    /// executes all deferred actions immediately.<br/>
    /// Calls <see cref="InvalidateCache"/> upon completion
    /// </para>
    /// </summary>
    internal void Unlock()
    {
        _locks -= 1;
        if (IsLocked) 
            return;
        
        foreach (var entity in _entityRemoveQueue) 
            RemoveEntity(entity);
        _entityRemoveQueue.Clear();
        
        foreach (var (entity, componentType) in _componentRemoveQueue) 
            RemoveEntityComponent(entity, componentType);
        _componentRemoveQueue.Clear();
        
        foreach (var (entity, component, componentType) in _componentAddQueue) 
            AddEntityComponent(entity, component, componentType);
        _componentAddQueue.Clear();
    }

    private int GetComponentId(Type component) => ComponentManager.GetIdByType(component);
    
    /// <inheritdoc cref="ArchetypeManager.Query(Signature, Signature)"/>
    internal IEnumerable<Archetype> QueryArchetypes(Signature include, Signature exclude)
        => ArchetypeManager.Query(include, exclude);

    #region Public Api

    /// <summary>
    /// Append a <paramref name="system"/> to the update list.<br/>
    /// <see cref="ISystem.OnInit(World)"/> is called immediately.
    /// </summary>
    /// <param name="system"></param>
    public void AddSystem(ISystem system) => SystemManager.RegisterSystem(system);

    /// <summary>
    /// Updates all systems in order.
    /// </summary>
    public void Update() => SystemManager.UpdateAll();

    #region Entity Interactions
    
    /// <summary>
    /// Initializes a new <c>Entity</c> in the world using<br/>
    /// information from the <c>Assembly</c>
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns><c>Entity</c> that was created as a result</returns>
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
    /// <param name="entity"></param>
    public void RemoveEntity(Entity entity)
    {
        if (IsLocked)
        {
            Logger.Logger.Debug(this, $"Deferred removal of {entity}");
            _entityRemoveQueue.Add(entity);
            return;
        }
        ArchetypeManager.RemoveEntity(entity);
        _entities.Remove(entity);
        
        InvalidateCache();
    }

    // TODO: Cringe ass idk son im crine
    /// <summary>
    /// Attempts to add a <paramref name="component"/> for <paramref name="entity"/>.<br/>
    /// Errors and does nothing if <paramref name="entity"/> already has <paramref name="componentType"/>.<br/>
    /// If <see cref="World.IsLocked"/> is true, defers the operation.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="component"></param>
    /// <param name="componentType"></param>
    private void AddEntityComponent(Entity entity, IComponent component, Type componentType)
    {
        if (!_entities.Contains(entity))
        {
            Logger.Logger.Error(this, $"{entity} doesn't exist");
            return;
        }
        if (IsLocked)
        {
            Logger.Logger.Debug(this, $"Deferred addition of {component} to {entity}");
            _componentAddQueue.Add((entity, component, componentType));
            return;
        }
        
        Archetype currentArchetype = ArchetypeManager.GetArchetypeByEntity(entity);
        Signature targetSignature = currentArchetype.Signature.Set(GetComponentId(componentType));

        if (targetSignature.Equals(currentArchetype.Signature))
        {
            Logger.Logger.Error(this, $"{entity} already has {componentType}. Use {nameof(ModifyEntityComponent)}");
            return;
        }

        Archetype targetArchetype = ArchetypeManager.GetOrAddArchetypeBySignature(targetSignature);

        ArchetypeManager.MoveEntity(entity, currentArchetype, targetArchetype);
        targetArchetype.SetComponent(entity, component);
        
        InvalidateCache();

        Logger.Logger.Debug(this, $"Added {componentType} to {entity}");
    }

    /// <inheritdoc cref="AddEntityComponent(Entity, IComponent, Type)(Entity, Type)"/>
    /// <param name="entity">Entity.</param>
    /// <param name="component">The component to add.</param>
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
        if (!_entities.Contains(entity))
        {
            Logger.Logger.Error(this, $"{entity} doesn't exist");
            return;
        }
        if (IsLocked)
        {
            _componentRemoveQueue.Add((entity, componentType));
            return;
        }

        Archetype currentArchetype = ArchetypeManager.GetArchetypeByEntity(entity);
        Signature targetSignature = currentArchetype.Signature.Unset(GetComponentId(componentType));

        if (targetSignature.Equals(currentArchetype.Signature))
        {
            Logger.Logger.Error(this, $"{entity} doesn't have {componentType}");
            return;
        }

        Archetype targetArchetype = ArchetypeManager.GetOrAddArchetypeBySignature(targetSignature);

        ArchetypeManager.MoveEntity(entity, currentArchetype, targetArchetype);

        InvalidateCache();
        
        Logger.Logger.Debug(this, $"Removed {componentType} from {entity}");
    }
    
    /// <typeparam name="TComponent">The component type to remove.</typeparam>
    /// <inheritdoc cref="RemoveEntityComponent(Entity, Type)"/>
    public void RemoveEntityComponent<TComponent>(Entity entity) where TComponent : struct, IComponent
    {
        RemoveEntityComponent(entity, typeof(TComponent));
    }

    #endregion

    #region Heavy Api

    /// <summary>
    /// <para>
    /// Gets a concrete component of <paramref name="entity"/>
    /// </para>
    /// <para>
    /// <i>May have unwanted performance impact if used in hot loops</i><br/>
    /// Performs multiple dictionary probes
    /// </para>
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    /// <param name="entity"></param>
    /// <returns><typeparamref name="TComponent"/> if found successfully, <c>null</c> otherwise</returns>
    public TComponent? GetEntityComponent<TComponent>(Entity entity) where TComponent : struct, IComponent
    {
        Logger.Logger.Info(this, $"Getting {entity} component {nameof(TComponent)} manually");
        if (!_entities.Contains(entity))
        {
            Logger.Logger.Error(this, $"{entity} doesn't exist");
            return null;
        }

        Archetype archetype = ArchetypeManager.GetArchetypeByEntity(entity);
        if (!archetype.Signature.IsSet(GetComponentId(typeof(TComponent))))
        {
            Logger.Logger.Error(this, $"{entity} doesn't have {typeof(TComponent)}");
            return null;
        }

        return archetype.GetComponent<TComponent>(entity);
    }

    /// <summary>
    /// <para>
    /// Applies <paramref name="modifier"/> func to <typeparamref name="TComponent"/> of <paramref name="entity"/>
    /// </para>
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    /// <param name="entity"></param>
    /// <param name="modifier"></param>
    /// <returns><c>true</c> if operation is successful, <c>false</c> otherwise.</returns>
    public bool ModifyEntityComponent<TComponent>(Entity entity, Func<TComponent, TComponent> modifier) where TComponent : struct, IComponent
    {
        if (!_entities.Contains(entity))
        {
            Logger.Logger.Error(this, $"{entity} doesn't exist");
            return false;
        }

        Archetype archetype = ArchetypeManager.GetArchetypeByEntity(entity);
        archetype.ModifyComponent(entity, modifier);
        return true;
    }
    /// <summary>
    /// Doesn't check if <paramref name="entity"/> exists for convenience.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>All <paramref name="entity"/> components.</returns>
    public IEnumerable<IComponent> GetEntityComponents(Entity entity)
    {
        return ArchetypeManager.GetArchetypeByEntity(entity).GetEntityComponents(entity);
    }
    
    /// <summary>
    /// <b>Extremely performance-heavy operation.</b><br/>
    /// Collects all entities and their components.
    /// </summary>
    public IEnumerable<(Entity Entity, List<IComponent> Components)> GetAllEntities()
    {
        Logger.Logger.Info(this, "Collecting all entities");
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