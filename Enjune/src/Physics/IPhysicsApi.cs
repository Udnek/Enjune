using Enjune.Ecs;
using Enjune.Ecs.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enjune.Physics;

public interface IPhysicsApi
{
    void RegisterSystems(World world);
}
