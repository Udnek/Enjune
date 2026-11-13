using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System.Misc;
using Minecraft2.Terrain;
using Utils = Minecraft2.Misc.Utils;

namespace Minecraft2.Ecs.System.Chunks;

public class ChunkCheckPendingNeighborsSystem : AppSystem
{
    private Query<ChunkComponent, ChunkGraphicComponent> _query = null!;

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Retrieve<ChunkComponent, ChunkGraphicComponent>();
    }

    public override void OnUpdate()
    {
        _query.ForEach((_, ref chunkComp, ref graphicLink) =>
        {
            byte pending = 0;
            var pos = chunkComp.Chunk.Position;
            foreach (var offset in Utils.FaceNeighbors)
            {
                if (App.ChunkWorld.IsQueued(pos + offset))
                    pending++;
            }
            graphicLink.PendingNeighbors = pending;
        });
    }
}