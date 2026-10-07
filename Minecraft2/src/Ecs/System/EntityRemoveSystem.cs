using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Ecs.System;

public class EntityRemoveSystem : AppSystem
{
    private Query<ToBeRemovedRequest> _query;
    private World _world;

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
            _world.RemoveEntity(entity);
        });
    }
}