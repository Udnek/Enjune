using Enjune.Ecs.Component;
using Enjune.Ecs.EcsType;
using Enjune.Misc;

namespace Enjune.Ecs.System;

public class GravitySystem : BaseSystem
{
    protected override Signature GenerateSignature(SignatureBuilder builder)
    {
        return builder
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
                Logger.Log(this, $"added gravitational acceleration to entity {archetype.GetIdByRow(i)}");
            }
        });
    }
}