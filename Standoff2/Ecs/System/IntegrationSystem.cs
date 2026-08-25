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
        Query.ForEachArchetype(archetype =>
        {
            Span<Position> positions = archetype.GetComponents<Position>();
            Span<Velocity> velocities = archetype.GetComponents<Velocity>();
            Span<Acceleration> accelerations = archetype.GetComponents<Acceleration>();

            for (int i = 0; i < archetype.Rows; i++)
            {
                const float dt = 0.01f;
                Logger.Info(this, $"processing {archetype.GetEntityByRow(i)} with params:\n" +
                                      $"- - - - Position:     {positions[i].ToString()}\n" +
                                      $"- - - - Velocity:     {velocities[i].ToString()}\n" +
                                      $"- - - - Acceleration: {accelerations[i].ToString()}");
                // First we integrate positions
                positions[i].X += dt * velocities[i].X;
                positions[i].Y += dt * velocities[i].Y;
                positions[i].Z += dt * velocities[i].Z;

                // Then we integrate velocities
                velocities[i].X += dt * accelerations[i].X;
                velocities[i].Y += dt * accelerations[i].Y;
                velocities[i].Z += dt * accelerations[i].Z;

                // Lastly, we reset all accelerations
                accelerations[i].X = 0;
                accelerations[i].Y = 0;
                accelerations[i].Z = 0;
            }
        });
    }
}