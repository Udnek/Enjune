using System;
using Enjune.Physics.EcsType;
using Enjune.Physics.Manager;
using Enjune.Physics.System;

namespace Enjune.Physics;

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

    public void Initialize()
    {
        SystemManager.InitializeSystems();
    }

    public void AddEntity(EntityAssembly assembly)
    {
        Archetype archetype = ArchetypeManager.GetArchetype(assembly.GetSignature(this));
        archetype.AddEntity(assembly);
    }

    public void Update<TSystem>() where TSystem : ISystem
    {
        SystemManager.Update<TSystem>(this);
    }

    public Signature ConstructSignature(Action<SignatureBuilder> configure)
    {
        var builder = new SignatureBuilder(this);
        configure(builder);
        return builder.Build();
    }

    public void QueryToUpdate(Signature signature, Action<Archetype> update)
    {
        ArchetypeManager.Query(signature, update);
    }
}