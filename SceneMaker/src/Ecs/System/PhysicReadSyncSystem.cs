using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using SceneMaker.Bridge;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Ecs.System;

public class PhysicReadSyncSystem(PhysicBridge bridge): SingleQuerySystem
{
    protected override Query BuildQuery(Query.Builder builder)
    {
        return builder
            .With<Transform>().Build();
    }

    public override void Update(World world)
    {
        var graphicObjs = bridge.Objects;
        Query.ForEachArchetype(archetype =>
        {
            var transforms = archetype.GetComponents<Transform>();
            for (int i = 0; i < archetype.Rows; i++)
            {
                var transform = transforms[i];
                var obj = graphicObjs[entity];

                transform.Position = obj.Position;
                transform.Rotation = obj.Rotation;

                transforms[i] = transform;
            }
            archetype.ForEachRowAndEntity((i, entity) =>
            {

            });
        });
    }
}
