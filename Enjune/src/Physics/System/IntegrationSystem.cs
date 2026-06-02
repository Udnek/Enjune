using Enjune.Misc;
using Enjune.Physics.Component;
using Enjune.Physics.EcsType;
using Enjune.Physics.Manager;
using Enjune.Physics.System;
using Enjune.World;

namespace Enjune.Physics.System;

public class IntegrationSystem : ISystem {

    public Signature Initialize(SignatureBuilder builder)
    {
        return builder.RegisterComponent<Component.Position>()
            .RegisterComponent<Velocity>()
            .RegisterComponent<Acceleration>()
            .Build();
    }
    
    public void Update(Archetype archetype)
    {
        Span<Component.Position> positions = archetype.GetComponents<Component.Position>();
        Span<Velocity> velocities = archetype.GetComponents<Velocity>();
        Span<Acceleration> accelerations = archetype.GetComponents<Acceleration>();

        for (int i = 0; i < archetype.EntityCount; i++)
        {
            Logger.Log(this, $"processing entity {archetype.GetIdByRow(i)} with params:\n" +
                                  $"- - - - Position:     {positions[i].ToString()}\n" +
                                  $"- - - - Velocity:     {velocities[i].ToString()}\n" +
                                  $"- - - - Acceleration: {accelerations[i].ToString()}");
            // First we integrate positions
            positions[i].X += EcsConstants.DeltaTime * velocities[i].X;
            positions[i].Y += EcsConstants.DeltaTime * velocities[i].Y;
            positions[i].Z += EcsConstants.DeltaTime * velocities[i].Z;

            // Then we integrate velocities
            velocities[i].X += EcsConstants.DeltaTime * accelerations[i].X;
            velocities[i].Y += EcsConstants.DeltaTime * accelerations[i].Y;
            velocities[i].Z += EcsConstants.DeltaTime * accelerations[i].Z;

            // Lastly, we reset all accelerations
            accelerations[i].X = 0;
            accelerations[i].Y = 0;
            accelerations[i].Z = 0;
        }
    }
}