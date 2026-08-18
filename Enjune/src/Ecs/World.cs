using System.Data;
using System.Runtime.InteropServices;
using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.Manager;
using Enjune.Ecs.System;
using Enjune.Misc;

namespace Enjune.Ecs;

public sealed class World
{
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

    public void AddEntityComponent(Entity entity, IComponent component)
    {
        if (!_entities.Contains(entity))
        {
            Logger.Error(this, $"AddEntityComponent: {entity} doesn't exist"); 
            return;
        }
        Archetype currentArchetype = ArchetypeManager.GetArchetypeByEntity(entity);
        Signature targetSignature = currentArchetype.Signature.Set(GetComponentId(component));

        Archetype targetArchetype = ArchetypeManager.GetOrAddArchetypeBySignature(targetSignature);

        ArchetypeManager.MoveEntity(entity, currentArchetype, targetArchetype);
        targetArchetype.WriteComponent(entity, component);

        Logger.Info(this, $"{nameof(AddEntityComponent)}: Added component successfully");
    }

    public int GetComponentId(IComponent component)
    {
        return (int)ComponentManager.GetTypeIdByType(component.GetType());
    }

    // Don't use in hot loops
    public List<(Entity, List<IComponent>)> GetAllEntities()
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
    #endregion

    #endregion
}