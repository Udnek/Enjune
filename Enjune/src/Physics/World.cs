using System;
using Enjune.Physics.EcsType;
using Enjune.Physics.Manager;

namespace Enjune.Physics;

public static class World
{
    public static readonly ArchetypeManager ArchetypeManager = new ArchetypeManager();
    public static readonly ComponentManager ComponentManager = new ComponentManager();
    public static readonly SystemManager SystemManager = new SystemManager();
    public static readonly EntityManager EntityManager = new EntityManager();

    public static void AddEntity(EntityAssembly assembly)
    {
        Archetype archetype = ArchetypeManager.GetArchetype(assembly.GetSignature());
        archetype.AddEntity(assembly);
    }
}