using Enjune.Ecs;
using Enjune.Physics;
using PhysicsLibrary.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcsPhysicsEngine;

public class PhysicsApi : IPhysicsApi
{
    public static void RegisterSystems(World world)
    {
        // Ordered
        world.AddSystem(new GravitySystem());
        
        
        // Should be the last
        world.AddSystem(new IntegrationSystem());
    }
}
