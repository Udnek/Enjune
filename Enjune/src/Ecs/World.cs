using Enjune.Ecs.EcsType;
using Enjune.Ecs.Manager;
using Enjune.Ecs.System;

namespace Enjune.Ecs;

public class World
{
    public readonly ArchetypeManager ArchetypeManager;
    public readonly SystemManager SystemManager;
    public readonly ComponentManager ComponentManager = new ComponentManager();
    public readonly EntityManager EntityManager = new EntityManager();

    public World()
    {
        // Can't use "this" in member initializer :P
        ArchetypeManager = new ArchetypeManager(this);
        SystemManager = new SystemManager(this);
    }
    
    // PUBLIC API
    public void AddEntity(EntityAssembly assembly) => ArchetypeManager.AddEntity(assembly);

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