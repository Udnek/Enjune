namespace Enjune.Physics.Type;

public readonly record struct Entity(EntityId Id)
{
    public readonly EntityId Id = Id;
}
