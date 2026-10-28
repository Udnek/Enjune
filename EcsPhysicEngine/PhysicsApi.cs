using Enjune.Ecs;
using PhysicsLibrary.System;

namespace PhysicsLibrary;

public class PhysicsApi
{
    public static void RegisterSystems(World world)
    {
        // Ordered
        world.AddSystem(new GravitySystem());
        
        
        // Should be the last
        world.AddSystem(new IntegrationSystem());
    }
}
