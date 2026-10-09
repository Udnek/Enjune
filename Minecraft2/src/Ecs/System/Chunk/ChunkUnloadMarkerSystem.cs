using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Ecs.System.Chunk;

public class ChunkUnloadMarkerSystem : AppSystem
{
    private Query<ChunkComponent> _query = null!;

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Retrieve<ChunkComponent>();
    }

    public override void OnUpdate()
    {
        _query.ForEach((_, ref chunkComp) =>
        {
            chunkComp.ToBeUnloaded = true;
        });
    }
}