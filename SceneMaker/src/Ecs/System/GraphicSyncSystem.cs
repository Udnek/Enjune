using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using SceneMaker.Bridge;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Ecs.System;

public class GraphicSyncSystem(GraphicBridge bridge) : SingleQuerySystem
{
    protected override Query BuildQuery(Query.Builder builder)
    {
        return builder
            .With<ModelComponent>()
            .With<Transform>().Build();
    }

    public override void Update(World world)
    {
        var graphicObjs = bridge.Objects;
        Query.ForEachArchetype(archetype =>
        {
            var models = archetype.GetComponents<ModelComponent>();
            var transforms = archetype.GetComponents<Transform>();
            for (int i = 0; i < archetype.EntityCount; i++) //TODO iterate over entityId
            {
                var entity = archetype.GetEntityByRow(i);
                var model = models[i];
                var transform = transforms[i];
                var obj = graphicObjs[entity];
                
                obj.TransformMatrix =
                    MathUtils.CreateModelTransform(transform.Position, transform.Rotation, transform.Scale);
                obj.DropsShadow = model.DropsShadow;
                obj.Hidden = model.IsHidden;
                
                graphicObjs[entity] = obj;
            }
        });
    }
}