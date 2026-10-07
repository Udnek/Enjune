using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Minecraft2.Bridge;
using Minecraft2.Ecs.Component;
using Minecraft2.Misc;

namespace Minecraft2.Ecs.System;

public class ChunkManageModelSystem : AppSystem
{
    private RecyclingPool<DynamicRenderableModel> _chunkModelPool = null!;
    private Query<ChunkComponent> _toAddQuery = null!;
    private Query<GraphicLinkComponent> _toRemoveQuery = null!;
    private World _world = null!;

    public override void OnInit(World world)
    {
        _world = world;
        _toAddQuery = new QueryBuilder(world)
            .Excluding<ToBeRemovedRequest>()
            .Excluding<GraphicLinkComponent>()
            .Retrieve<ChunkComponent>();

        _toRemoveQuery = new QueryBuilder(world)
            .Including<ToBeRemovedRequest>()
            .Including<ChunkComponent>()
            .Retrieve<GraphicLinkComponent>();
        
        _chunkModelPool = new RecyclingPool<DynamicRenderableModel>(() => new DynamicRenderableModel(App.GraphicApi));
    }

    public override void OnUpdate()
    {
        var graphicObjects = App.GraphicEngine.Objects;

        _toRemoveQuery.ForEach((entity, ref graphicLink) =>
        {
            if (graphicObjects.Remove(graphicLink.GraphicId, out var graphicObject))
                _chunkModelPool.Recycle((DynamicRenderableModel)graphicObject.Model);
            else
                Logger.Warn(this, $"{entity} has {graphicLink} but doesn't appear in {nameof(GraphicEngine)}");

            _world.RemoveEntityComponent<GraphicLinkComponent>(entity);
        });
        
        _toAddQuery.ForEach((entity, ref chunkComp) =>
        {
            var chunk = chunkComp.Chunk;
            if (chunk.MarkedUnloaded) return;
            var graphicLink = new GraphicLinkComponent();
            _world.AddEntityComponent(entity, graphicLink);
            var graphicObject = new GraphicObject
            {
                Model = _chunkModelPool.Take(),
                TransformMatrix = MathUtils.CreateModelTransform(
                    chunk.Position * Chunk.Size,
                    Quaternion.Identity,
                    Vector3.One)
            };

            graphicObjects[graphicLink.GraphicId] = graphicObject;
        });
    }
}