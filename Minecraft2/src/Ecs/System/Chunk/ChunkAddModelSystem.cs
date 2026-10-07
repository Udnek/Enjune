using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Minecraft2.Bridge;
using Minecraft2.Ecs.Component;
using Minecraft2.Misc;
using OpenTK.Mathematics;

namespace Minecraft2.Ecs.System;

public class ChunkAddModelSystem : AppSystem
{
    private World _world = null!;
    private Query<ChunkComponent> _query = null!;

    public override void OnInit(World world)
    {
        _world = world;
        _query = new QueryBuilder(world)
            .Excluding<GraphicLinkComponent>()
            .Retrieve<ChunkComponent>();
    }

    public override void OnUpdate()
    {
        var graphicObjects = App.GraphicEngine.Objects;
        
        _query.ForEach((entity, ref chunkComp) =>
        {
            if (chunkComp.ToBeUnloaded) return;
            
            var chunk = chunkComp.Chunk;
            var graphicLink = new GraphicLinkComponent();
            _world.AddEntityComponent(entity, graphicLink);
            var graphicObject = new GraphicObject
            {
                Model = App.ChunkModelPool.Take(),
                TransformMatrix = MathUtils.CreateModelTransform(
                    chunk.Position * Chunk.Size,
                    Quaternion.Identity,
                    Vector3.One)
            };

            graphicObjects[graphicLink.GraphicId] = graphicObject;
        });
    }
}