using Enjune.Data;
using Enjune.Data.Codec;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.Manager;
using Enjune.Ecs.System;
using Enjune.Misc;
using Enjune.Registering;

namespace Enjune.Ecs;

public sealed class World
{
    public static readonly ICodec<World> WithoutSystemsCodec = new SimpleCodec<World>(world =>
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

            var world = new World([], componentTypes);
            foreach (var assembly in entities) 
                world.AddEntity(assembly);
            return ResultOrError.Success(world);
        }
        );
    
    internal readonly ArchetypeManager ArchetypeManager;
    internal readonly SystemManager SystemManager;
    internal readonly ComponentManager ComponentManager = new ComponentManager();
    internal readonly EntityManager EntityManager = new EntityManager();

    // This cache version marks broad archetype structure version:
    // it increments when a new archetype gets created, but does not
    // increment when archetype's entity container changes
    internal int ArchetypeCacheVersion { get; private set; } = 0;
    private List<Entity> _entities = [];

    internal void InvalidateArchetypeCache()
    {
        Logger.Info(this, "Invalidated archetype cache");
        ArchetypeCacheVersion++;
    }

    public World(IEnumerable<ISystem> systems, IEnumerable<Type> componentTypes)
    {
        Logger.Info(this, "Registering managers using given systems and component types");

        ArchetypeManager = new ArchetypeManager(this);
        SystemManager = new SystemManager(this);
        
        foreach (Type componentType in componentTypes) 
            ComponentManager.RegisterComponentType(componentType);
        
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

        return entity;
    }

    public void RemoveEntity(Entity entity)
    {
        ArchetypeManager.RemoveEntity(entity);
        _entities.Remove(entity);
    }

    public int GetComponentId(Type component)
    {
        return (int)ComponentManager.GetIdByType(component);
    }

    public void AddEntityComponent(Entity entity, IComponent component)
    {
        if (!_entities.Contains(entity)) { Logger.Error(Logger.Domain.Ecs, $"{this}.{nameof(AddEntityComponent)}", $"{entity} doesn't exist"); return; }
        Archetype currentArchetype = ArchetypeManager.GetArchetypeByEntity(entity);
        Signature targetSignature = currentArchetype.Signature.Set(GetComponentId(component.GetType()));

        if (targetSignature.Equals(currentArchetype.Signature)) { Logger.Warn(this, $"Trying to add a component that already exists"); }

        Archetype targetArchetype = ArchetypeManager.GetOrAddArchetypeBySignature(targetSignature);

        ArchetypeManager.MoveEntity(entity, currentArchetype, targetArchetype);
        targetArchetype.WriteComponent(entity, component);

        Logger.Info(this, $"{nameof(AddEntityComponent)}: Added component successfully");
    }

    public void RemoveEntityComponent<TComponent>(Entity entity) where TComponent : struct, IComponent
    {
        if (!_entities.Contains(entity)) { Logger.Error(Logger.Domain.Ecs, $"{this}.{nameof(RemoveEntityComponent)}", $"{entity} doesn't exist"); return; }
        Archetype currentArchetype = ArchetypeManager.GetArchetypeByEntity(entity);
        Signature targetSignature = currentArchetype.Signature.Unset(GetComponentId(typeof(TComponent)));

        if (targetSignature.Equals(currentArchetype.Signature)) { Logger.Warn(this, $"Trying to remove a component that doesn't exist"); }

        Archetype targetArchetype = ArchetypeManager.GetOrAddArchetypeBySignature(targetSignature);

        ArchetypeManager.MoveEntity(entity, currentArchetype, targetArchetype);

        Logger.Info(this, $"{nameof(AddEntityComponent)}: Removed component successfully");
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
        Logger.Warn("Sergeant Hartman", "Do you suck dicks?!");
        return snapshots;
    }

    // Tries to create an OptionalRef with component value inside. Contains nothing in case of failure (HasValue is false)
    public OptionalRef<TComponent> TryGetEntityComponent<TComponent>(Entity entity) where TComponent : struct, IComponent
    {
        Archetype archetype = ArchetypeManager.GetArchetypeByEntity(entity);
        return archetype.TryGetEntityComponent<TComponent>(entity);
    }
    #endregion

    #endregion
}