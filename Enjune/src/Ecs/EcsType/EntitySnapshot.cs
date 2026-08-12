using Enjune.Ecs.Component;

namespace Enjune.Ecs.EcsType;

public class EntitySnapshot
{
    public EntityId Id { get; }
    public List<IComponent> Components = [];

    public EntitySnapshot(EntityId id)
    {
        Id = id;
    }


}

