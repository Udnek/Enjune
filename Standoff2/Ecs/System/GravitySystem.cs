using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using Standoff2.Ecs.Component;

namespace Standoff2.Ecs.System;

public class GravitySystem : SingleQuerySystem
{
    
    protected override Query BuildQuery(Query.Builder builder)
    {
        return builder.With<Acceleration>().Build();
    }

    public override void Update()
    {
        Query.ForEach((ref Acceleration acc) =>
        {
            // Simply add -9,80665 to Y acceleration
            // TODO: Consider changing this behavior to something more accurate
            acc.Y -= 9.80665;
            Logger.Info(this, $"Added gravitational acceleration to entity");
        });
    }
}