using Enjune.Ecs.Component;
using Enjune.Misc;
using System;
using System.Text;

namespace Enjune.Ecs.EcsType;

public readonly record struct Entity
{
    private readonly uint _id;

    public Entity(uint id) => _id = id;
    public Entity(int id) => _id = (uint)id;
}
