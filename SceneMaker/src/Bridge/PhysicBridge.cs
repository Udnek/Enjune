using Enjune.Ecs.EcsType;
using Enjune.Physic;

namespace SceneMaker.Bridge;

public class PhysicBridge
{
    public readonly Dictionary<Entity, IPhysicObject> Objects = [];
}