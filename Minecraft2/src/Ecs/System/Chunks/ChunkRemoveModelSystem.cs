using System.Threading.Channels;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System.Misc;

namespace Minecraft2.Ecs.System.Chunks;

public class ChunkRemoveModelSystem : AppSystem
{
    private World _world = null!;
    private Query<ChunkComponent, GraphicLinkComponent> _query = null!;

    public override void OnInit(World world)
    {
        _world = world;
        _query = new QueryBuilder(world)
            .Retrieve<ChunkComponent, GraphicLinkComponent>();
    }

    public override void OnUpdate()
    {
        _query.ForEach((entity, ref chunkComp, ref graphicLink) =>
        {
            if (!chunkComp.ToBeUnloaded) return;
            
            // removing api model
            App.GraphicEngine.EnqueueChunkRemoval(graphicLink.GraphicId);
            // recycling local model
            App.ChunkModelPool.Recycle(graphicLink.Model);
            
            _world.RemoveEntityComponent<GraphicLinkComponent>(entity);
        });
    }
}