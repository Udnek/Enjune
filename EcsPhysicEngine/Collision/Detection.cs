using PhysicsLibrary.Collision.Component;
using PhysicsLibrary.General.Component;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PhysicsLibrary.Collision;

/// <summary>
/// Presents methods that only <b>detect</b> collision,
/// usually returning a true/false value.
/// </summary>
public static partial class Detection
{
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
        Transform Transform1, AABB Box1,
        Transform Transform2, AABB Box2
        )
    {
        // Boxes intersect at a coordinate if their center positions'
        // delta is less or equal to sum of their extents
        bool intersectX = MathF.Abs(Transform1.Position.X - Transform2.Position.X) <= Box1.Extents.X + Box2.Extents.X;
        bool intersectY = MathF.Abs(Transform1.Position.Y - Transform2.Position.Y) <= Box1.Extents.Y + Box2.Extents.Y;
        bool intersectZ = MathF.Abs(Transform1.Position.Z - Transform2.Position.Z) <= Box1.Extents.Z + Box2.Extents.Z;
        return intersectX & intersectY & intersectZ;
    }
}
