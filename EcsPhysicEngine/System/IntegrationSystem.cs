using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using PhysicsLibrary.Component;

namespace PhysicsLibrary.System;

public class IntegrationSystem : ISystem
{
    private Query<Position, Velocity, Acceleration> _query = null!;
    public void OnInit(World world)
    {
        _query = new QueryBuilder(world).Retrieve<Position, Velocity, Acceleration>();
    }

    public void OnUpdate()
    {
        _query.ForEach((
            entity,
            ref pos,
            ref vel,
            ref acc) =>
        {
            const float dt = 0.01f;
            Logger.Info(this, $"Processing {entity} with params:\n" +
                                  $"- - - - Position:     {pos}\n" +
                                  $"- - - - Velocity:     {vel}\n" +
                                  $"- - - - Acceleration: {acc}");
            // Integrate positions
            pos.X += dt * vel.X;
            pos.Y += dt * vel.Y;
            pos.Z += dt * vel.Z;

            // Integrate velocities
            vel.X += dt * acc.X;
            vel.Y += dt * acc.Y;
            vel.Z += dt * acc.Z;

            // Reset all accelerations
            acc.X = 0;
            acc.Y = 0;
            acc.Z = 0;
        });
    }
}
