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
    public void AddEntity(EntityAssembly assembly)
    {
        Entity entity = EntityManager.CreateEntity();
        ArchetypeManager.AddEntity(assembly, entity);
    }

    public void RemoveEntity(Entity entity) => ArchetypeManager.RemoveEntity(entity);

    // very slow-poke
    public List<Entity.Snapshot> GetAllEntities()
    {
        List<Entity.Snapshot> snapshots = [];
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