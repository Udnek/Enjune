using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using OpenTK.Mathematics;
using static System.MathF;

namespace Enjune.Misc;

public static class MathUtils
{
    public const float Epsilon = 1e-6f; // don't fucking touch it, increasing accuracy may brake everything
    
    public static bool RayIntersectsLine(Position origin, Vector3 direction, Position p0, Position p1, out float cosDistance)
    {
        p0 -= origin;
        p1 -= origin;
        var projected = ProjectVectorOnPlane(direction, Vector3.Zero, p0, p1);
        // so if projection lies between two points of line, then sum of angles must be equals to angle between points of line
        var leftAngle = Acos(CosAngleBetween(projected, p0));
        var rightAngle = Acos(CosAngleBetween(projected, p1));
        var sumAngle = Acos(CosAngleBetween(p0, p1));
        if (Abs(sumAngle - (leftAngle + rightAngle)) < Epsilon*10) // increasing epsilon cos Acos sucks
        {
            cosDistance = CosAngleBetween(direction, projected);
            return true;
        }
        cosDistance = 0;
        return false;
    }

    // https://en.wikipedia.org/wiki/Cramer%27s_rule
    public static bool VectorsIntersect(Position originA, Vector3 directionA, Position originB, Vector3 directionB, out Position intersection)
    {
        // solving for k and m
        // origA + k*dirA = origB + m*dirB 
        // k*dirA - m*dirB = origB -origA
        // matrix A: (dirA  -dirB | origB - origA)
        // vector b: origB - origA 
        // x_i = det(Ai)/det(A)
        // Ai = i-th column replaced with b
        
        var b = originB - originA;
        
        // dropping z-coord to cause overdetermined
        var detXy = Det(directionA.Xy, -directionB.Xy);
        var detXz = Det(directionA.Xz, -directionB.Xz);
        var detYz = Det(directionA.Yz, -directionB.Yz);

        float k;
        if (Abs(detXy) >= Abs(detXz) && Abs(detXy) >= Abs(detYz))
            k = FindK(b.Xy, -directionB.Xy, detXy);
        else if (Abs(detXz) >= Abs(detXy) && Abs(detXz) >= Abs(detYz))
            k = FindK(b.Xz, -directionB.Xz, detXz);
        else
            k = FindK(b.Yz, -directionB.Yz, detYz);

        if (k <= 0)
        {
            intersection = originA;
            return false;
        }
        //Logger.Log(typeof(MathUtils), $"{originB}");

        intersection = originA + directionA * k;
        return true;
        

        static float Det(Vector2 column1, Vector2 column2) 
            => column1.X * column2.Y - column2.X * column1.Y;

        static float FindK(Vector2 b, Vector2 secondColumn, float detA)
        {
            if (Abs(detA) < Epsilon)
            {
                Logger.Error($"{nameof(MathUtils)}.{nameof(VectorsIntersect)}", $"detA = {detA}");
                return -1;
            }
            var detAi = Det(b, secondColumn);
            return detAi / detA;
        }
    }
    
    public static float DegreeAngleBetween(Vector3 a, Vector3 b) 
        => MathHelper.RadiansToDegrees(Acos(CosAngleBetween(a, b)));
    
    public static float CosAngleBetween(Vector3 a, Vector3 b) 
        => Vector3.Dot(a, b) / a.Length / b.Length;

    public static Vector3 ProjectVectorOnPlane(Vector3 direction, Position p0, Position p1, Position p2)
    {
        var planeNorm = PlaneNormNotNormalized(p0, p1, p2);
        if (planeNorm.LengthSquared < Epsilon)
        {
            Logger.Error($"{nameof(MathUtils)}.{nameof(ProjectVectorOnPlane)}", $"plane norm is too small: {planeNorm}");
            return direction;
        }
        return direction - ProjectAonB(direction, planeNorm);
    }

    public static Vector3 ProjectAonB(Vector3 a, Vector3 b)
    {
        if (b.LengthSquared < Epsilon)
        {
            Logger.Error($"{nameof(MathUtils)}.{nameof(ProjectAonB)}", $"b vec is too small: {b}");
            return a;
        }
        b.Normalize();
        return Vector3.Dot(a, b) * b;
    }
    
    public static Vector3 PlaneNormNotNormalized(Position p0, Position p1, Position p2) 
        => Vector3.Cross(p1-p0, p2-p0);
    
    public static float PointToVectorDistance(Position p, Vector3 direction) 
        => Vector3.Cross(p, direction.Normalized()).Length;

    // https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
    public static bool RayIntersectsTriangle(Position origin, Vector3 direction, Position p0, Position p1, Position p2, out float distance)
    {
        distance = 0;
        
        var e1 = p1 - p0;
        var e2 = p2 - p0;

        var pvec = Vector3.Cross(direction, e2);
        var det = Vector3.Dot(e1, pvec);

        // parallel
        if (det < Epsilon && det > -Epsilon) 
            return false;

        var invDet = 1 / det;
        var tvec = origin - p0;
        var u = Vector3.Dot(tvec, pvec) * invDet;
        if (u < 0 || 1 < u) 
            return false;

        var qvec = Vector3.Cross(tvec, e1);
        float v = Vector3.Dot(direction, qvec) * invDet;
        if (v < 0 || 1 < u + v)
            return false;
        
        distance = Vector3.Dot(e2, qvec) * invDet;
        return true;
    }
}