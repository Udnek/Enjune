using Enjune.Physics.EcsType;
using Enjune.Physics.Component;

namespace Enjune.Physics.System;

public class GravitySystem : ISystem
{
    private Signature _signature;
    public GravitySystem()
    {
        _signature = new SignatureBuilder()
            .RegisterComponent<Acceleration>()
            .Build();
    }

    public void Update()
    {
        Archetype archetype = World.ArchetypeManager.GetArchetype(_signature);
        Span<Acceleration> accelerations = archetype.GetComponents<Acceleration>();
        
        for (int i = 0; i < archetype.EntityCount; i++)
        {
            // Simply add -9,80665 to Y acceleration
            // TODO: The thing below is a placeholder
            accelerations[i].Y += EcsConstants.GravitationalAcceleration;
        }
    }
}