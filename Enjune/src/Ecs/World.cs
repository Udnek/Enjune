using Enjune.Attribute;
using Enjune.Data;
using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.Manager;
using Enjune.Ecs.System;
using Enjune.Misc;

namespace Enjune.Ecs;

[LogParams(logCallingMethod:true)]
public sealed class World
{
    public static readonly ICodec<World> WithoutSystemsCodec = new SimpleCodec<World>(
        world =>
        {
            var allEntities = world.GetAllEntities();
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

    internal void InvalidateCache()
    {
        Logger.Info(this, "Invalidated cache");
        CacheVersion++;
    }

    public World(IEnumerable<ISystem> systems)
    {
        Logger.Info(this, "Registering managers");

        ArchetypeManager = new ArchetypeManager(this);
        SystemManager = new SystemManager(this);
        
        foreach (ISystem system in systems) 
            SystemManager.RegisterSystem(system);
    }

    #region Public Api

    public void AddSystem(ISystem system) => SystemManager.RegisterSystem(system);

    public void Update() => SystemManager.UpdateAll();
    
    public IEnumerable<Archetype> QueryArchetypes(Signature include, Signature exclude)
        => ArchetypeManager.Query(include, exclude);


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

    public int GetComponentId(Type component)
    {
        return (int)ComponentManager.GetIdByType(component);
    }

    // Don't use in hot loops
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
            Logger.Error(this, $"{entity} already has {component.GetType()}. Use {nameof(ModifyEntityComponent)}");
            return false;
        }

        Archetype targetArchetype = ArchetypeManager.GetOrAddArchetypeBySignature(targetSignature);

        ArchetypeManager.MoveEntity(entity, currentArchetype, targetArchetype);
        targetArchetype.SetComponent(entity, component);

        Logger.Info(this, $"Added {component.GetType()} to {entity}");
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
            Logger.Error(this, $"{entity} doesn't have {typeof(TComponent)}");
            return false;
        }

        Archetype targetArchetype = ArchetypeManager.GetOrAddArchetypeBySignature(targetSignature);

        ArchetypeManager.MoveEntity(entity, currentArchetype, targetArchetype);

        Logger.Info(this,$"Removed {typeof(TComponent)} from {entity} successfully");
        InvalidateCache();
        return true;
    }

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
        InvalidateCache();
        return true;
    }

    // Don't use in hot loops
    public List<(Entity Entity, List<IComponent> Components)> GetAllEntities()
    {
        List<(Entity, List<IComponent>)> snapshots = [];
        foreach (var archetype in QueryArchetypes(Signature.Empty, Signature.Empty))
        {
            foreach (var snapshot in archetype.GetAllEntitySnapshots())
            {
                snapshots.Add(snapshot);
            }
        }
        return snapshots;
    }

    #endregion

    #endregion
}