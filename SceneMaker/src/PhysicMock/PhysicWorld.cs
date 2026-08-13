using Enjune.Physic;

namespace SceneMaker.PhysicMock;

public class PhysicWorld : IPhysicWorld
{
    public IPhysicObject CreateObject() => new PhysicObject();
}