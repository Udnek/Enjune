using Enjune.Physic;

namespace SceneMaker.PhysicMock;

public class PhysicObject : IPhysicObject
{
    public int Mass { get; set; } = 1;
    public IPhysicObject.BodyType Type => IPhysicObject.BodyType.Dynamic;
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public void AddForce(Vector3 force) { }

    public void Remove() { }
}