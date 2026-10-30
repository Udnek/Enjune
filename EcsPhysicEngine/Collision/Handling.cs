using PhysicsLibrary.Collision.Component;
using PhysicsLibrary.General.Component;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace PhysicsLibrary.Collision;
/// <summary>
/// Presents methods that perform a full collision resolution pipeline,<br/>
/// checking and applying collision in-place.<br/>
/// This approach is generally more effective at scenes with a low object count
/// </summary>
internal class Handling
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MinimumTranslationVector(
        ref Transform TfmA, AABB BoxA, Mass MassA,
        ref Transform TfmB, AABB BoxB, Mass MassB
        )
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool GetMTV(
            Transform TfmA, AABB BoxA,
            Transform TfmB, AABB BoxB,
            out Vector3 normal, out float depth)
        {
            float depthX = BoxA.Extents.X + BoxB.Extents.X - MathF.Abs(TfmA.Position.X - TfmB.Position.X);
            float depthY = BoxA.Extents.Y + BoxB.Extents.Y - MathF.Abs(TfmA.Position.Y - TfmB.Position.Y);
            float depthZ = BoxA.Extents.Z + BoxB.Extents.Z - MathF.Abs(TfmA.Position.Z - TfmB.Position.Z);

            if (depthX <= 0 || depthY <= 0 || depthZ <= 0) { 
                normal = Vector3.Zero;
                depth = 0;
                return false;
            }

            if (depthX <= depthY && depthX <= depthZ)
            {
                depth = depthX;
                normal = (TfmB.Position.X >= TfmA.Position.X) ? Vector3.UnitX : -Vector3.UnitX;
            }
            else if (depthY <= depthZ)
            {
                depth = depthY;
                normal = (TfmB.Position.Y >= TfmA.Position.Y) ? Vector3.UnitY : -Vector3.UnitY;
            }
            else
            {
                depth = depthZ;
                normal = (TfmB.Position.Z >= TfmA.Position.Z) ? Vector3.UnitZ : -Vector3.UnitZ;
            }
            return true;
        }
        if (!GetMTV(TfmA, BoxA, TfmB, BoxB, out Vector3 normal, out float depth)) { return; } // No collision

        float totalInverseMass = MassA.Inverse + MassB.Inverse;
        if (totalInverseMass <= 0) { return; } // Both objects are static

        Vector3 mtv = normal * depth;
        TfmA.Position -= mtv * (MassA.Inverse / totalInverseMass);
        TfmB.Position += mtv * (MassB.Inverse / totalInverseMass);
    }
}
