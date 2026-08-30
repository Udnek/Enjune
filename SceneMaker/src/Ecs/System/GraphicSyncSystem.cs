using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using SceneMaker.Bridge;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Ecs.System;

public class GraphicSyncSystem(GraphicEngine engine) : SingleQuerySystem
{
    protected override Query BuildQuery(Query.Builder builder)
    {
        return builder
            .With<ModelComponent>()
            .With<Transform>().Build();
    }

    public override void Update()
    {
        var graphicObjs = engine.Objects;
        Query.ForEachArchetype(archetype =>
        {
            var models = archetype.GetComponents<ModelComponent>();
            var transforms = archetype.GetComponents<Transform>();
            for (int i = 0; i < archetype.Rows; i++)
            {
                var model = models[i];
                var transform = transforms[i];
                var obj = graphicObjs[model.GraphicId];

                obj.TransformMatrix = transform.Matrix;
                obj.DropsShadow = model.DropsShadow;
                obj.IsHidden = model.IsHidden;
                
                graphicObjs[model.GraphicId] = obj;
            }
        });
    }
}