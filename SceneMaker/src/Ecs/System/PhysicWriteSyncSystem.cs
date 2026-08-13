using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using SceneMaker.Bridge;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Ecs.System;

public class PhysicWriteSyncSystem(PhysicBridge bridge): BaseSystem
{
    protected override Signature GenerateSignature(Signature.Builder builder)
    {
        return builder
            .RegisterComponent<Transform>().Build();
    }

    public override void Update(World world)
    {
        var graphicObjs = bridge.Objects;
        world.QueryToUpdate(Signature, archetype =>
        {
            var transforms = archetype.GetComponents<Transform>();
            for (int i = 0; i < archetype.EntityCount; i++) //TODO iterate over entityId
            {
                var entityId = archetype.GetIdByRow(i);
                var transform = transforms[i];
                var obj = graphicObjs[entityId];

                obj.Position = transform.Position;
                obj.Rotation = transform.Rotation;

                graphicObjs[entityId] = obj;
            }
        });
    }
}
