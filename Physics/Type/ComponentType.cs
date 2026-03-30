namespace Enjune.Physics.Type;

public readonly record struct ComponentType(ComponentTypeId Id)
{
    public readonly ComponentTypeId Id = Id;
}