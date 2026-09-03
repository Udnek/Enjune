using Enjune.Ecs;
using Enjune.Physic;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcsPhysicsEngine;

internal class PhysicsWorld : IPhysicsWorld
{
    private World _world;

    public IPhysicObject CreateObject()
    {
        throw new NotImplementedException();
    }
}
