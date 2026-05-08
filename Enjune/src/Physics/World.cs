using System;
using Enjune.Physics.Manager;

namespace Enjune.Physics;

public class World
{
    public static readonly ArchetypeManager ArchetypeManager = new ArchetypeManager();
    public static readonly ComponentManager ComponentManager = new ComponentManager();
    public static readonly SystemManager SystemManager = new SystemManager();
    public static readonly EntityManager EntityManager = new EntityManager();
    public void Test()
    {
        Console.WriteLine(EcsConstants.MaxEntities);
    }
}