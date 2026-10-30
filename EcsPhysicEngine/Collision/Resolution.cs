using PhysicsLibrary.Collision.Component;
using PhysicsLibrary.General.Component;
using System.Runtime.CompilerServices;

namespace PhysicsLibrary.Collision;
/// <summary>
/// Presents methods that only <b>resolve</b> collisions,<br/>
/// by either modifying given data or returning values that should be applied manually.<br/>
/// Methods would expect objects to be colliding, not performing
/// any additional tests
/// </summary>
public static class Resolution
{
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static void MinimumTranslationVector(
    //    ref Transform Transform1, AABB Box1,
    //    ref Transform Transform2, AABB Box2
    //    )
    //{
    //    double depthX = Box1.Extents.X + Box2.Extents.X - MathF.Abs(Transform1.Position.X - Transform2.Position.X);
    //    double depthY = Box1.Extents.Y + Box2.Extents.Y - MathF.Abs(Transform1.Position.Y - Transform2.Position.Y);
    //    double depthZ = Box1.Extents.Z + Box2.Extents.Z - MathF.Abs(Transform1.Position.Z - Transform2.Position.Z);
    //}
}
