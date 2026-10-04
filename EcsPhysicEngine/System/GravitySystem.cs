using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using PhysicsLibrary.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhysicsLibrary.System;
public class GravitySystem : ISystem
{
    private Query<Acceleration> _query = null!;
    public void OnInit(World world)
    {
        _query = new QueryBuilder(world).Retrieve<Acceleration>();
    }
    public void OnUpdate()
    {
        _query.ForEach((Entity entity, ref Acceleration acc) =>
        {
            // Simply add -9,80665 to Y acceleration
            // TODO: Consider changing this behavior to something more accurate
            acc.Y -= 9.80665;
            Logger.Info(this, $"Added gravitational acceleration to {entity}");
        });
    }
}
