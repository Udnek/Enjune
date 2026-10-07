using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Ecs.System;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using Minecraft2.Misc;
using OpenTK.Mathematics;

namespace Minecraft2.Ecs.System;

public class ChunkLoadSystem : AppSystem
{
    private Query<Transform, ChunkLoader> _query;
    private World _world;

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
            var rad = loader.Radius;
            var center = Chunk.ToChunkPos(transform.Position);
            for (int x = -rad; x <= rad; x++)
            {
                for (int z = -rad; z <= rad; z++)
                {
                    var chunk = chunkWorld.Load(center + (x, 0, z), out var wasAlreadyLoaded);
                    if (wasAlreadyLoaded) continue;
                    _world.AddEntity(new Entity.Assembly()
                        .AddComponent(new ChunkComponent { Chunk = chunk }));
                }
            }
        });
    }
}