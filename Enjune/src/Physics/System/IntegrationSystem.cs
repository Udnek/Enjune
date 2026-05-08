using Enjune.Physics.Component;
using Enjune.Physics.EcsType;
using Enjune.Physics.Manager;
using Enjune.Physics.System;
using Enjune.World;

namespace Enjune.Physics.System;

public class IntegrationSystem : ISystem
{
    private Signature _signature;
    public IntegrationSystem()
    {
        _signature = new SignatureBuilder()
            .RegisterComponent<Component.Position>()
            .RegisterComponent<Velocity>()
            .RegisterComponent<Acceleration>()
            .Build();
    }
    
    // TODO: Check if archetype is empty
    public void Update()
    {
        Archetype integrationArchetype = World.ArchetypeManager.GetArchetype(_signature);

        Span<Component.Position> positions = integrationArchetype.GetComponents<Component.Position>();
        Span<Velocity> velocities = integrationArchetype.GetComponents<Velocity>();
        Span<Acceleration> accelerations = integrationArchetype.GetComponents<Acceleration>();

        for (int i = 0; i < integrationArchetype.EntityCount; i++)
        {
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