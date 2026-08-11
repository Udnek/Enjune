using System.Data;
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
    public void AddEntity(EntityAssembly assembly)
    {
        EntityId id = EntityManager.CreateEntity();
        ArchetypeManager.AddEntity(assembly, id);
    }

    public void RemoveEntity(EntityId id) => ArchetypeManager.RemoveEntity(id);

    public void Update() => SystemManager.UpdateAll();

    public Signature ConstructSignature(Action<SignatureBuilder> configure)
    {
        var builder = new SignatureBuilder(this);
        configure(builder);
        return builder.Build();
    }

    public void QueryToUpdate(Signature signature, Action<Archetype> update) 
        => ArchetypeManager.Query(signature, update);
}