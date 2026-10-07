using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Minecraft2.Ecs.Component;

namespace Minecraft2.Ecs.System;

// TODO
public class PlayerControllerSystem : AppSystem
{
    private Query<Transform> _query;

    public override void OnInit(World world)
    {
        _query = new QueryBuilder(world)
            .Including<PlayerControlled>()
            .Retrieve<Transform>();
    }

    public override void OnUpdate()
    {
        _query.ForEach((_, ref trans) =>
        {
            
        });
    }
}