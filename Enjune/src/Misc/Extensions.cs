using System.Diagnostics.Contracts;
using System.Globalization;
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

    public static decimal? ParseDecimalOrNull(this string str)
    {
        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        return null;
    }
    
    public static string SafeSubstringLen(this string str, int from, int len)
        => str.SafeSubstringFromTo(from, from + len);
    public static string SafeSubstringFromTo(this string str, int from, int to)
    {
        if (str.Length == 0) return string.Empty;
        from = Math.Clamp(from, 0, str.Length - 1);
        to = Math.Clamp(to, 0, str.Length);
        if (from == to) return "";
        if (from > to) 
            (from, to) = (to, from);
        return str[from..to];
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

    public static void ForEachIndexed<T>(this List<T> list, Action<int, T> action)
    {
        for (var i = 0; i < list.Count; i++) 
            action(i, list[i]);
    }
    
    public static void ForEach<T>(this T[] array, Action<T> action)
    {
        foreach (var t in array) action(t);
    }

    public static void Lock(this Mutex mutex, Action action)
    {
        mutex.WaitOne();
        action();
        mutex.ReleaseMutex();
    }
}


