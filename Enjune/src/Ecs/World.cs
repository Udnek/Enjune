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

    public int CacheVersion { get; private set; } = 0;

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

    internal void InvalidateCache()
    {
        Logger.Info(this, "World cache invalidated.");
        CacheVersion++;
    }

    // PUBLIC API
    public void AddEntity(EntityAssembly assembly)
    {
        EntityId id = EntityManager.CreateEntity();
        ArchetypeManager.AddEntity(assembly, id);
    }

    public void RemoveEntity(Entity id) => ArchetypeManager.RemoveEntity(id);

    public void Update() => SystemManager.UpdateAll();

    public Signature ConstructSignature(Action<Signature.Builder> configure)
    {
        var builder = new Signature.Builder(this);
        configure(builder);
        return builder.Build();
    }

    public void QueryToUpdate(Signature signature, Action<Archetype> update) 
        => ArchetypeManager.ForEachMatched(signature, update);

    public List<Entity.Snapshot> GetAllEntities()
    {
        List<Entity.Snapshot> snapshots = [];

        ArchetypeManager.ForEachMatched(Signature.Empty, archetype =>
        {
            foreach (var snapshot in archetype.GetAllEntities())
            {
                snapshots.Add(snapshot);
            }
        });
        return snapshots;
    }
}