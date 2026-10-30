using OpenTK.Mathematics;

namespace PhysicsLibrary.Collision.Component;

public record struct AABB
{
    public Vector3 Extents { get; set; }
}