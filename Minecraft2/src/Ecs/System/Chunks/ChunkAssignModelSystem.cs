using System.Collections.Concurrent;
using System.Threading.Channels;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System.Misc;
using Minecraft2.Terrain;
using Utils = Minecraft2.Ecs.System.Misc.Utils;

namespace Minecraft2.Ecs.System.Chunks;

public class ChunkAssignModelSystem : AppSystem
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

            var pos = chunkComp.Chunk.Position;
            int pendingNeighbors = 0;
            Utils.ForFaceNeighbors(offset =>
            {
                if (App.ChunkWorld.IsQueued(pos + offset))
                    pendingNeighbors++;
            });
            var graphicLink = new ChunkGraphicComponent
            {
                Model = App.ChunkModelPool.Take(),
                PendingNeighbors = pendingNeighbors
            };
            _world.AddEntityComponent(entity, graphicLink);

            App.GraphicEngine.EnqueueChunkAddition(
                graphicLink.GraphicId,
                MathUtils.CreateModelTransform(
                    pos * Chunk.Size,
                    Quaternion.Identity,
                    Vector3.One));
        });
    }
}