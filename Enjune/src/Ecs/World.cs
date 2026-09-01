using Enjune.Attribute;
using Enjune.Data;
using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.Manager;
using Enjune.Ecs.System;
using Enjune.Misc;

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
            HashSet<Type> componentTypes = [];
            foreach (var compsData in array.Val)
            {
                var result = IComponent.ArrayCodec.Decode(compsData);
                if (result.Error != null)
                    return new Error($"can not decode components: {result.Error}");
                var assembly = new Entity.Assembly();
                foreach (var comp in result.GetOrThrow())
                {
                    assembly.AddComponent(comp);
                    componentTypes.Add(comp.GetType());
                }
                entities.Add(assembly);
            }

            var world = new World([]);
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
    internal int CacheVersion { get; private set; } = 0;
    private List<Entity> _entities = [];

    public World(IEnumerable<ISystem> systems)
    {
        Logger.Info(this, "Registering managers using given systems and component types");

        ArchetypeManager = new ArchetypeManager(this);
        SystemManager = new SystemManager(this);
        
        foreach (ISystem system in systems) 
            SystemManager.RegisterSystem(system);
    }

    private void InvalidateCache()
    {
        Logger.Info(this, "Invalidated cache");
        CacheVersion++;
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

    public void RemoveEntity(Entity entity)
    {
        ArchetypeManager.RemoveEntity(entity);
        _entities.Remove(entity);
        InvalidateCache();
    }
    
    public bool AddEntityComponent(Entity entity, IComponent component)
    {
        if (!_entities.Contains(entity)) 
        { 
            Logger.Error(this, $"{entity} doesn't exist"); 
            return false; 
        }
        Archetype currentArchetype = ArchetypeManager.GetArchetypeByEntity(entity);
        Signature targetSignature = currentArchetype.Signature.Set(GetComponentId(component.GetType()));

        if (targetSignature.Equals(currentArchetype.Signature)) 
        {
            Logger.Error(this, $"Tried to add component {component} that already exists");
            return false;
        }

        Archetype targetArchetype = ArchetypeManager.GetOrAddArchetypeBySignature(targetSignature);

        ArchetypeManager.MoveEntity(entity, currentArchetype, targetArchetype);
        targetArchetype.SetComponent(entity, component);

        Logger.Info(this,  $"Added component {component} successfully");
        InvalidateCache();
        return true;
    }

    // Don't use in hot loops
    public bool RemoveEntityComponent<TComponent>(Entity entity) where TComponent : struct, IComponent
    {
        if (!_entities.Contains(entity)) 
        { 
            Logger.Error(this, $"{entity} doesn't exist"); 
            return false; 
        }
        Archetype currentArchetype = ArchetypeManager.GetArchetypeByEntity(entity);
        Signature targetSignature = currentArchetype.Signature.Unset(GetComponentId(typeof(TComponent)));

        if (targetSignature.Equals(currentArchetype.Signature)) 
        { 
            Logger.Error(this, $"Trying to remove a component {typeof(TComponent)} that doesn't exist");
            return false;
        }

        Archetype targetArchetype = ArchetypeManager.GetOrAddArchetypeBySignature(targetSignature);

        ArchetypeManager.MoveEntity(entity, currentArchetype, targetArchetype);

        Logger.Info(this, $"{nameof(AddEntityComponent)}: Removed component successfully");
        InvalidateCache();
        return true;
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
        return archetype.GetComponentCopy<TComponent>(entity);
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
        InvalidateCache();
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