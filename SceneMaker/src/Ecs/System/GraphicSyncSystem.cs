using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using SceneMaker.Bridge;
using SceneMaker.Ecs.Component;

namespace SceneMaker.Ecs.System;

public class GraphicSyncSystem(GraphicEngine engine) : ISystem
{
    private Query<ModelComponent, Transform> _modelQuery = null!;
    private Query<SpotLightComponent, Transform> _spotLightQuery = null!;
    private Query<ModelComponent> _selectedInEditorQuery = null!;

    public void OnInit(World world)
    {
        _modelQuery = new QueryBuilder(world)
            .Retrieve<ModelComponent, Transform>();

        _spotLightQuery = new QueryBuilder(world)
            .Retrieve<SpotLightComponent, Transform>();

        _selectedInEditorQuery = new QueryBuilder(world)
            .Including<SelectedInEditor>()
            .Retrieve<ModelComponent>();
    }

    public void OnUpdate()
    {
        #region Models
        {
            var graphicObjs = engine.Objects;
            _modelQuery.ForEach((_, ref model, ref transform) =>
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
            _spotLightQuery.ForEach((_, ref light, ref transform) =>
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
            _selectedInEditorQuery.ForEach((_, ref model) =>
            {
                var obj = graphicObjects[model.GraphicId];
                obj.IsHighlighted = true;
                graphicObjects[model.GraphicId] = obj;
            });
        }
        #endregion
    }
}