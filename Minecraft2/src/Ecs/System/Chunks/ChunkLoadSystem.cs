using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System.Misc;
using Minecraft2.Terrain;

namespace Minecraft2.Ecs.System.Chunks;

public class ChunkLoadSystem : AppSystem
{
    private Query<Transform, ChunkLoader> _query = null!;
    private World _world = null!;

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Retrieve<Transform, ChunkLoader>();
        _world = world;
    }

    public override void OnUpdate()
    {
        var chunkWorld = App.ChunkWorld;
        _query.ForEach((_, ref transform, ref loader) =>
        {
            var xzRadius = loader.HorizontalRadius;
            var yRadius = loader.VerticalRadius;
            var center = Chunk.ToChunkPos(transform.Position);
            for (int x = -xzRadius; x <= xzRadius; x++)
            {
                for (int z = -xzRadius; z <= xzRadius; z++)
                {
                    for (int y = -yRadius; y <= yRadius; y++)
                    {
                        chunkWorld.QueueLoading(center + (x, y, z));
                    }
                }
            }
        });
        // pulling from generator
        chunkWorld.PullFromGenerator();
    }
}