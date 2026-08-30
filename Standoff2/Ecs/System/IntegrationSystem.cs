using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using Standoff2.Ecs.Component;

namespace Standoff2.Ecs.System;

public class IntegrationSystem : SingleQuerySystem
{
    protected override Query BuildQuery(Query.Builder builder)
    {
        return builder
            .With<Position>()
            .With<Velocity>()
            .With<Acceleration>()
            .Build();
    }

    public override void Update(World world)
    {
        Query.ForEach((
            ref Position pos,
            ref Velocity vel,
            ref Acceleration acc) =>
        {
            const float dt = 0.01f;
            Logger.Info(this, $"processing entity? with params:\n" +
                                  $"- - - - Position:     {pos.ToString()}\n" +
                                  $"- - - - Velocity:     {vel.ToString()}\n" +
                                  $"- - - - Acceleration: {acc.ToString()}");
            // First we integrate positions
            pos.X += dt * vel.X;
            pos.Y += dt * vel.Y;
            pos.Z += dt * vel.Z;
            
            // Then we integrate velocities
            vel.X += dt * acc.X;
            vel.Y += dt * acc.Y;
            vel.Z += dt * acc.Z;
            
            // Lastly, we reset all accelerations
            acc.X = 0;
            acc.Y = 0;
            acc.Z = 0;
        });
    }
}