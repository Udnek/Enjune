using OpenTK.Mathematics;

namespace PhysicsLibrary.Collision.Component;

public interface IAxisAlignedBoundingBox
{
    public Vector3 Extents { get; set; }
    public double XExtent { get; set; }
    public double YExtent { get; set; }
    public double ZExtent { get; set; }
}