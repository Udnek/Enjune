namespace Enjune.Ecs.EcsType;



public struct Command
{
    enum Kind
    {
        AddEntity,
        RemoveEntity,
        RemoveComponent
    }
}

