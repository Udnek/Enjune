using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using SceneMaker.Bridge;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Ecs.System;

public class GraphicSyncSystem(GraphicEngine engine) : ISystem
{
    private Query _modelQuery = null!;
    private Query _spotLightQuery = null!;
    private Query _selectedInEditorQuery = null!;

    public void Initialize(World world)
    {
        _modelQuery = Query.For(world)
            .With<ModelComponent>()
            .With<Transform>().Build();
        
        _spotLightQuery = Query.For(world)
            .With<SpotLightComponent>()
            .With<Transform>().Build();

        _selectedInEditorQuery = Query.For(world)
            .With<SelectedInEditor>()
            .With<ModelComponent>().Build();
    }

    public void Update()
    {
        #region Models
        {
            var graphicObjs = engine.Objects;
            _modelQuery.ForEach((ref ModelComponent model, ref Transform transform) =>
            {
                var obj = graphicObjs[model.GraphicId];

                obj.TransformMatrix = transform.Matrix;
                obj.DropsShadow = model.DropsShadow;
                obj.IsHidden = model.IsHidden;
                
                graphicObjs[model.GraphicId] = obj;
            });
        }
        #endregion
        
        #region SpotLights
        {
            var graphicSpotLights = engine.SpotLights;
            _spotLightQuery.ForEach((ref SpotLightComponent light, ref Transform transform) =>
            {
                var graphicLight = graphicSpotLights[light.GraphicId];

                graphicLight.Projection = light.Projection;
                graphicLight.Color = light.Color;
                graphicLight.Position = transform.Position;
                graphicLight.View = MathUtils.CreateView(transform.Position, transform.Rotation);

                graphicSpotLights[light.GraphicId] = graphicLight;
            });
        }
        #endregion
        
        #region Selected
        {
            var graphicObjects = engine.Objects;
            // un-highlighting
            foreach (var (key, _) in graphicObjects)
            {
                var obj = graphicObjects[key];
                obj.IsHighlighted = false;
                graphicObjects[key] = obj;
            }
            // highlighting
            _selectedInEditorQuery.ForEach((ref ModelComponent model) =>
            {
                var obj = graphicObjects[model.GraphicId];
                obj.IsHighlighted = true;
                graphicObjects[model.GraphicId] = obj;
            });
        }
        #endregion
    }
}