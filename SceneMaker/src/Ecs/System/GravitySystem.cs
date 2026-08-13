using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Ecs.System;

public class GravitySystem : BaseSystem
{
    protected override Signature GenerateSignature(Signature.Builder builder)
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
                accelerations[i].Y -= 10; // EGE moment
                Logger.Info(this, $"added gravitational acceleration to entity {archetype.GetEntityByRow(i)}");
            }
        });
    }
}