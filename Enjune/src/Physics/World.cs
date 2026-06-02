using System;
using Enjune.Physics.EcsType;
using Enjune.Physics.Manager;

namespace Enjune.Physics;

// TODO what is the purpose apart from composing if they do not interact with each other. I idk anyway
public static class World
{
    public static readonly ArchetypeManager ArchetypeManager = new ArchetypeManager();
    public static readonly ComponentManager ComponentManager = new ComponentManager();
    public static readonly SystemManager SystemManager = new SystemManager();
    public static readonly EntityManager EntityManager = new EntityManager();
    public static readonly QueryManager QueryManager = new QueryManager();

    public static void AddEntity(EntityAssembly assembly)
    {
        Archetype archetype = ArchetypeManager.GetArchetype(assembly.GetSignature());
        archetype.AddEntity(assembly);
    }
}