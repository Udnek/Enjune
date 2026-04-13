namespace Enjune.Physics.EcsType;

public readonly record struct Entity(EntityId Id)
{
    public readonly EntityId Id = Id;
}
