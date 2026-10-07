using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Ecs.System;

public class EntityRemoveSystem : AppSystem
{
    private Query<ToBeRemovedRequest> _query = null!;
    private World _world = null!;

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Excluding<GraphicLinkComponent>()
            .Retrieve<ToBeRemovedRequest>();
        _world = world;
    }


    public override void OnUpdate()
    {
        _query.ForEach((entity, ref _) =>
        {
            Logger.Highlight(this, $"Removed {entity}");
            _world.RemoveEntity(entity);
        });
    }
}