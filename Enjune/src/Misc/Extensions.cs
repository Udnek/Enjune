using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    
    extension<T>(List<T> list)
    {
        public void RemoveLastIfNotEmpty()
        {
            if (list.Count == 0) return;
            list.RemoveAt(list.Count - 1);
        }

        public void ForEachIndexed(Action<int, T> action)
        {
            for (var i = 0; i < list.Count; i++) 
                action(i, list[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => CollectionsMarshal.AsSpan(list);
    }
    
    extension(string str)
    {
        public decimal? ParseDecimalOrNull()
        {
            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
            return null;
        }

        public string SafeSubstringLen(int from, int len)
            => str.SafeSubstringFromTo(from, from + len);

        public string SafeSubstringFromTo(int from, int to)
        {
            if (str.Length == 0) return string.Empty;
            from = Math.Clamp(from, 0, str.Length - 1);
            to = Math.Clamp(to, 0, str.Length);
            if (from == to) return "";
            if (from > to) 
                (from, to) = (to, from);
            return str[from..to];
        }
    }
    
    extension<T>(T[] array)
    {
        public (T, TOther)[] JoinToTuple<TOther>(TOther[] other)
            => array.Select((v, i) => (v, other[i])).ToArray();

        public void ForEach(Action<T> action)
        {
            foreach (var t in array) action(t);
        }
    }

    
    public static Assembly GetAssembly(this object obj) => obj.GetType().Assembly;

    public static Color ToTk(this System.Drawing.Color color) => new Color(color.R, color.G, color.B, color.A) / 255f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToTk(this System.Numerics.Vector4 vector) => new(vector.X, vector.Y, vector.Z, vector.W);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToTk(this System.Numerics.Vector3 vector) => new(vector.X, vector.Y, vector.Z);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToTk(this System.Numerics.Vector2 vector) => new(vector.X, vector.Y);
    
    public static string ContentToString<T>(this IEnumerable<T> array, string prefix="[", string separator = ", ", string postfix="]")
        => prefix+string.Join(separator, array.Select(v => v?.ToString() ?? "null"))+postfix;

    public static void Lock(this Mutex mutex, Action action)
    {
        mutex.WaitOne();
        action();
        mutex.ReleaseMutex();
    }
}


