using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Ecs.System.Chunk;

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
            var radius = loader.Radius;
            var center = Misc.Chunk.ToChunkPos(transform.Position);
            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    chunkWorld.Load(center + (x, 0, z), out var _);
                }
            }
        });
    }
}