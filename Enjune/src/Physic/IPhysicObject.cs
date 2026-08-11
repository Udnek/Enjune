namespace Enjune.Physic;

public interface IPhysicObject
{
    int Mass { get; set; }
    BodyType Type { get; }
    
    void AddForce(Vector3 force);

    enum BodyType
    {
        Dynamic,
        Kinematic
    }
}