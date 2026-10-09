using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Bridge;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Ecs.System.Chunk;

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

        int c = 0;
        _query.ForEach((_, ref _) => c+=1);
        
        _query.ForEach((entity, ref chunkComp) =>
        {
            if (chunkComp.ToBeUnloaded) return;
            
            var graphicLink = new GraphicLinkComponent();
            _world.AddEntityComponent(entity, graphicLink);
            var graphicObject = new GraphicObject
            {
                Model = App.ChunkModelPool.Take(),
                TransformMatrix = MathUtils.CreateModelTransform(
                    chunkComp.Pos * Misc.Chunk.Size,
                    Quaternion.Identity,
                    Vector3.One)
            };

            graphicObjects[graphicLink.GraphicId] = graphicObject;
            Logger.Highlight(this, $"Assigned {graphicLink.GraphicId} to {chunkComp}");
        });
    }
}