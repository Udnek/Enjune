using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Ecs;
using Enjune.Misc;
using Standoff2.Ecs.Component;

namespace Standoff2.Ecs.System;

public class GravitySystem : ISystem
{
    private Query<Acceleration> _query;
    public void OnInit(World world)
    {
        _query = new QueryBuilder(world).Retrieve<Acceleration>();
    }
    public void OnUpdate(World world)
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