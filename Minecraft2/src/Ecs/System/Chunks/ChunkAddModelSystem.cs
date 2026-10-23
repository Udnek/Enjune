using System.Collections.Concurrent;
using System.Threading.Channels;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Bridge;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System.Misc;

namespace Minecraft2.Ecs.System.Chunks;

public class ChunkAddModelSystem : AppSystem
{
    private World _world = null!;
    private Query<ChunkComponent> _query = null!;

    public override void OnInit(World world)
    {
        _world = world;
        _query = new QueryBuilder(world)
            .Excluding<ChunkGraphicComponent>()
            .Retrieve<ChunkComponent>();
    }

    public override void OnUpdate()
    {
        _query.ForEach((entity, ref chunkComp) =>
        {
            if (chunkComp.ToBeUnloaded) return;
            
            var graphicLink = new ChunkGraphicComponent
            {
                Model = App.ChunkModelPool.Take()
            };
            _world.AddEntityComponent(entity, graphicLink);

            App.GraphicEngine.EnqueueChunkAddition(
                graphicLink.GraphicId,
                MathUtils.CreateModelTransform(
                    chunkComp.Pos * Minecraft2.Misc.Chunk.Size,
                    Quaternion.Identity,
                    Vector3.One));
        });
    }
}