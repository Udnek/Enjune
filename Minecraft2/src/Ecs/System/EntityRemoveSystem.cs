using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.Misc;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System.Misc;

namespace Minecraft2.Ecs.System;

public class EntityRemoveSystem : AppSystem
{
    private Query _query = null!;
    private World _world = null!;

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Excluding<GraphicLinkComponent>()
            .Including<ToBeRemovedRequest>()
            .Retrieve();
        _world = world;
    }


    public override void OnUpdate()
    {
        _query.ForEach(entity =>
        {
            Logger.Highlight(this, $"Removed {entity}");
            _world.RemoveEntity(entity);
        });
    }
}