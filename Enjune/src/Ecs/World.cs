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
    public int ArchetypeCacheVersion { get; private set; } = 0;

    public void InvalidateArchetypeCache()
    {
        Logger.Info(this, "Invalidated archetype cache");
        ArchetypeCacheVersion++;
    }

    public World(List<ISystem> systems, List<Type> componentTypes)
    {
        Logger.Info(this, "Registering managers using given systems and component types");

        ArchetypeManager = new ArchetypeManager(this);
        SystemManager = new SystemManager(this);
        
        foreach (Type componentType in componentTypes) 
            ComponentManager.RegisterComponentType(componentType);
        
        foreach (ISystem system in systems) 
            SystemManager.RegisterSystem(system);
    }

    // PUBLIC API
    // Common
    public void Update() => SystemManager.UpdateAll();


    // Entity interactions
    public void AddEntity(EntityAssembly assembly)
    {
        Entity entity = EntityManager.CreateEntity();
        ArchetypeManager.AddEntity(assembly, entity);
    }

    public void RemoveEntity(Entity entity) => ArchetypeManager.RemoveEntity(entity);

    public List<Entity.Snapshot> GetAllEntities()
    {
        List<Entity.Snapshot> snapshots = [];

        ForEachMatchedArchetype(Signature.Empty, archetype =>
        {
            foreach (var snapshot in archetype.GetAllEntities())
            {
                snapshots.Add(snapshot);
            }
        });
        return snapshots;
    }


    // Archetype interactions
    public void ForEachMatchedArchetype(Signature signature, Action<Archetype> update)
    {
        foreach (var archetype in QueryArchetypesSimple(signature))
            update(archetype);
    }

    public IEnumerable<Archetype> QueryArchetypesSimple(Signature signature)
        => ArchetypeManager.QuerySimple(signature);

    public IEnumerable<Archetype> QueryArchetypes(Signature includeSignature, Signature excludeSignature)
        => ArchetypeManager.Query(includeSignature, excludeSignature);

    public List<Archetype> GetMatchedArchetypes(Signature includeSignature, Signature excludeSignature) {
        var archetypes = new List<Archetype>();
        foreach (var archetype in QueryArchetypes(includeSignature, excludeSignature))
            archetypes.Add(archetype);
        return archetypes;
    }

    
    // Component interactions
    public List<Type> DeconstructSignature(Signature signature)
        => ComponentManager.DeconstructSignature(signature);
    public Signature ConstructSignature(Action<Signature.Builder> configure)
    {
        var builder = new Signature.Builder(this);
        configure(builder);
        return builder.Build();
    }
}