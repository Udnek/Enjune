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
            _modelQuery.ForEachArchetype(archetype =>
            {
                var models = archetype.GetComponents<ModelComponent>();
                var transforms = archetype.GetComponents<Transform>();
                for (int row = 0; row < archetype.Rows; row++)
                {
                    var model = models[row];
                    var transform = transforms[row];
                    var obj = graphicObjs[model.GraphicId];

                    obj.TransformMatrix = transform.Matrix;
                    obj.DropsShadow = model.DropsShadow;
                    obj.IsHidden = model.IsHidden;
                
                    graphicObjs[model.GraphicId] = obj;
                }
            });
        }
        #endregion
        
        #region SpotLights
        {
            var graphicSpotLights = engine.SpotLights;
            _spotLightQuery.ForEachArchetype(archetype =>
            {
                var spotLights = archetype.GetComponents<SpotLightComponent>();
                var transforms = archetype.GetComponents<Transform>();
                for (int i = 0; i < archetype.Rows; i++)
                {
                    var light = spotLights[i];
                    var transform = transforms[i];
                    var graphicLight = graphicSpotLights[light.GraphicId];

                    graphicLight.Projection = light.Projection;
                    graphicLight.Color = light.Color;
                    graphicLight.Position = transform.Position;
                    graphicLight.View = MathUtils.CreateView(transform.Position, transform.Rotation);

                    graphicSpotLights[light.GraphicId] = graphicLight;
                }
            });
        }
        #endregion
        
        #region Selected
        {
            var graphicObjects = engine.Objects;
            // un-highlighting
            foreach (var kv in graphicObjects)
            {
                var obj = graphicObjects[kv.Key];
                obj.IsHighlighted = false;
                graphicObjects[kv.Key] = obj;
            }
            // highlighting
            _selectedInEditorQuery.ForEachArchetype(archetype =>
            {
                var models = archetype.GetComponents<ModelComponent>();
                for (int row = 0; row < archetype.Rows; row++)
                {
                    var obj = graphicObjects[models[row].GraphicId];
                    obj.IsHighlighted = true;
                    graphicObjects[models[row].GraphicId] = obj;
                }
            });
        }
        #endregion
    }
}