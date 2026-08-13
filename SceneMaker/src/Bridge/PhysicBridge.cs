using Enjune.Physic;

namespace SceneMaker.Bridge;

public class PhysicBridge
{
    public readonly Dictionary<EntityId, IPhysicObject> Objects = [];
}