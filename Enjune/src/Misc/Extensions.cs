using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Enjune.Misc;

public static class Extensions
{
    extension(Matrix4 matrix)
    {
        [Pure]
        public Vector3 TransformDirection(Vector3 vector) => Vector3.TransformVector(vector, matrix);
        [Pure]
        public Vector3 TransformPosition(Vector3 vector) => Vector3.TransformPosition(vector, matrix);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToTk(this System.Numerics.Vector4 vector) => new(vector.X, vector.Y, vector.Z, vector.W);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToTk(this System.Numerics.Vector3 vector) => new(vector.X, vector.Y, vector.Z);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToTk(this System.Numerics.Vector2 vector) => new(vector.X, vector.Y);
    
    public static string ContentToString<T>(this IEnumerable<T> array, string prefix="[", string separator = ", ", string postfix="]")
        => prefix+string.Join(separator, array.Select(v => v?.ToString() ?? "null"))+postfix;

    public static (T0, T1)[] JoinToTuple<T0, T1>(this T0[] array, T1[] other)
        => array.Select((v, i) => (v, other[i])).ToArray();
}