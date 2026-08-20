using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using SceneMaker.Bridge;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Ecs.System;

public class PhysicWriteSyncSystem(PhysicBridge bridge): SingleQuerySystem
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
            for (int i = 0; i < archetype.Rows; i++) //TODO iterate over entityId
            {
                var entity = archetype.GetEntityByRow(i);
                var transform = transforms[i];
                var obj = graphicObjs[entity];

                obj.Position = transform.Position;
                obj.Rotation = transform.Rotation;

                graphicObjs[entity] = obj;
            }
        });
    }
}
