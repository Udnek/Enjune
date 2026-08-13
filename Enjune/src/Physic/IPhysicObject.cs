namespace Enjune.Physic;

public interface IPhysicObject
{
    int Mass { get; set; }
    BodyType Type { get; }

    Position Position { get; set; }
    Quaternion Rotation { get; set; }

    void AddForce(Vector3 force);

    public void Remove();
    
    enum BodyType
    {
        Dynamic,
        Kinematic
    }
}