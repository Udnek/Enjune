using Enjune.Physics.EcsType;
using Enjune.Physics.Component;

namespace Enjune.Physics.System;

public class GravitySystem : ISystem
{
    public Signature Signature { get; }

    public GravitySystem()
    {
        Signature = new SignatureBuilder()
            .RegisterComponent<Acceleration>()
            .Build();
    }

    public void Update()
    {
        //Archetype archetype = World.ArchetypeManager.GetArchetype(Signature);
        foreach (var archetype in World.ArchetypeManager.GetArchetypes())
        {
            if (!archetype.Signature.Contains(Signature)) continue;
            Span<Acceleration> accelerations = archetype.GetComponents<Acceleration>();
            for (int i = 0; i < archetype.EntityCount; i++)
            {
                // Simply add -9,80665 to Y acceleration
                // TODO: Consider changing this behavior to something more... accurate? Flexible?
                accelerations[i].Y += EcsConstants.GravitationalAcceleration;
                Logger.Log(GetType(), $"added gravitational acceleration to entity {archetype.GetIdByRow(i)}");
            }
        }
    }
}