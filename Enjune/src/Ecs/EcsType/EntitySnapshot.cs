using System;
using System.Collections.Generic;
using System.Text;
using Enjune.Ecs.Component;

namespace Enjune.src.Ecs.EcsType;

public class EntitySnapshot
{
    public EntityId Id { get; }
    public List<IComponent> Components = [];

    public EntitySnapshot(EntityId id)
    {
        Id = id;
    }


}

