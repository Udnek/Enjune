using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Minecraft2.Bridge;
using Minecraft2.Ecs.Component;
using Minecraft2.Misc;

namespace Minecraft2.Ecs.System;

public class ChunkAddModelSystem : ISystem
{
    private readonly RecyclingPool<DynamicRenderableModel> _chunkModelPool;
    private Query<ChunkComponent> _query = null!;
    private World _world = null!;
    private readonly GraphicEngine _graphicEngine;

    public ChunkAddModelSystem(App app)
    {
        _graphicEngine = app.GraphicEngine;
        _chunkModelPool = new RecyclingPool<DynamicRenderableModel>(() => new DynamicRenderableModel(app.GraphicApi));
    }

    public void OnInit(World world)
    {
        _world = world;
        _query = new QueryBuilder(world)
            .Excluding<GraphicLinkComponent>()
            .Retrieve<ChunkComponent>();
    }

    public void OnUpdate()
    {
        _query.ForEach((entity, ref chunkComp) =>
        {
            var chunk = chunkComp.Chunk;
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

            _graphicEngine.Objects[graphicLink.GraphicId] = graphicObject;
        });
    }
}