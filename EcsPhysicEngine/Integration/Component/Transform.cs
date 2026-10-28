namespace PhysicsLibrary.Collision.Component;

public interface ITransform
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Rotation { get; set; }
}