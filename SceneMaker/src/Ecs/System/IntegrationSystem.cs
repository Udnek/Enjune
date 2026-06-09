using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Ecs.System;

public class IntegrationSystem : BaseSystem
{
    protected override Signature GenerateSignature(SignatureBuilder builder)
    {
        return builder
            .RegisterComponent<Component.Position>()
            .RegisterComponent<Velocity>()
            .RegisterComponent<Acceleration>()
            .Build();
    }

    public override void Update(World world)
    {
        world.QueryToUpdate(Signature, archetype =>
        {
            Span<Component.Position> positions = archetype.GetComponents<Component.Position>();
            Span<Velocity> velocities = archetype.GetComponents<Velocity>();
            Span<Acceleration> accelerations = archetype.GetComponents<Acceleration>();

            for (int i = 0; i < archetype.EntityCount; i++)
            {
                const float dt = 0.01f;
                Logger.Log(this, $"processing entity {archetype.GetIdByRow(i)} with params:\n" +
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