using OpenTK.Mathematics;
using PhysicsLibrary.Collision.Component;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;

namespace PhysicsLibrary.Collision.Operation;

public static class Collision
{
    // Use ITransform and IAABB instead?
    /// <summary>
    /// Checks if two AABB's collide
    /// </summary>
    /// <param name="Pos1">Position of the first object</param>
    /// <param name="Ext1">AABB's extents of the first object</param>
    /// <param name="Pos2">Position of the second object</param>
    /// <param name="Ext2">AABB's extents of the second object</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Check(
        Vector3 Pos1, Vector3 Ext1,
        Vector3 Pos2, Vector3 Ext2
        )
    {
        // Boxes intersect at a coordinate if their center positions'
        // delta is less or equal to sum of their extents
        bool intersectX = MathF.Abs(Pos1.X - Pos2.X) <= Ext1.X + Ext2.X;
        bool intersectY = MathF.Abs(Pos1.Y - Pos2.Y) <= Ext1.Y + Ext2.Y;
        bool intersectZ = MathF.Abs(Pos1.Z - Pos2.Z) <= Ext1.Z + Ext2.Z;
        return intersectX & intersectY & intersectZ;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetRepulsion(
        Vector3 Pos1, Vector3 Ext1,
        Vector3 Pos2, Vector3 Ext2
        )
    {
        double penX = MathF.Abs(Pos1.X - Pos2.X) - Ext1.X + Ext2.X;
        double penY = MathF.Abs(Pos1.Y - Pos2.Y) - Ext1.Y + Ext2.Y;
        double penZ = MathF.Abs(Pos1.Z - Pos2.Z) - Ext1.Z + Ext2.Z;
        return new Vector3(0,0,0);
    }
}