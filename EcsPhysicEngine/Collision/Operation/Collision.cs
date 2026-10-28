using System.ComponentModel.Design;
using PhysicsLibrary.Collision.Component;

namespace PhysicsLibrary.Collision.Operation;

public static class Collision
{
    public bool Check<T1, T2>(T1 t1, T2 t2) where T1 : struct, IAxisAlignedBoundingBox where T2 : struct, IAxisAlignedBoundingBox
    {
        return false;
    }
}