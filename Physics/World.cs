using System;
using Enjune.Physics.Manager;

namespace Enjune.Physics;

public class World
{
    public static ArchetypeManager ArchetypeManager = new ArchetypeManager();
    public static ComponentManager ComponentManager = new ComponentManager();
    public void Test()
    {
        Console.WriteLine(EcsConstants.MaxEntities);
    }
}