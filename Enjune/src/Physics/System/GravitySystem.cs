using Enjune.Misc;
using Enjune.Physics.EcsType;
using Enjune.Physics.Component;

namespace Enjune.Physics.System;

public class GravitySystem : BaseSystem
{
    public override void Initialize(SignatureBuilder assignedBuilder)
    {
        Signature = assignedBuilder
            .RegisterComponent<Acceleration>()
            .Build();
    }

    public override void Update(World world)
    {
        world.QueryToUpdate(Signature, archetype =>
        {
            Span<Acceleration> accelerations = archetype.GetComponents<Acceleration>();
            for (int i = 0; i < archetype.EntityCount; i++)
            {
                // Simply add -9,80665 to Y acceleration
                // TODO: Consider changing this behavior to something more accurate
                accelerations[i].Y += EcsConstants.GravitationalAcceleration;
                Logger.Log(GetType(), $"added gravitational acceleration to entity {archetype.GetIdByRow(i)}");
            }
        });
    }
}