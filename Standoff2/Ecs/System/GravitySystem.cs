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
        Query.ForEachArchetype(archetype =>
        {
            Span<Acceleration> accelerations = archetype.GetComponents<Acceleration>();
            for (int i = 0; i < archetype.Rows; i++)
            {
                // Simply add -9,80665 to Y acceleration
                // TODO: Consider changing this behavior to something more accurate
                accelerations[i].Y += -10;
                Logger.Info(this, $"Added gravitational acceleration to {archetype.GetEntityByRow(i)}");
            }
        });
    }
}