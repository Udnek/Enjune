using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace Enjune.Misc;

public delegate void Consumer<in T>(T obj);

public static class Utils
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
    
    public static string ContentToString<T>(this IEnumerable<T> array)
        => "["+string.Join(", ", array.Select(v => v?.ToString() ?? "null"))+"]";

    public static (T0, T1)[] JoinToTuple<T0, T1>(this T0[] array, T1[] other)
        => array.Select((v, i) => (v, other[i])).ToArray();

    private static readonly float NanosInSec = 1_000_000_000.0f;
    public static Nanoseconds FpsToNanoDelay(Fps fps) => (Nanoseconds)(NanosInSec / fps);
    public static float NanosToSeconds(Nanoseconds nanos) => nanos / NanosInSec;
    public static Fps NanoDelayToFps(Nanoseconds nanos) => NanosInSec / nanos;
    private static Nanoseconds TicksToNanos(long ticks) => (Nanoseconds)(ticks * (NanosInSec / Stopwatch.Frequency));


    
    public static void RunTargetFpsLoopWhile(
        Fps targetFps, out float deltaTime, Consumer<Nanoseconds> delayConsumer, Func<bool> shouldContinue, Action action)
    {
        var targetDelay = FpsToNanoDelay(targetFps);
        
        var frameStart = TicksToNanos(Stopwatch.GetTimestamp());
        while (true)
        { 
            deltaTime = NanosToSeconds(TicksToNanos(Stopwatch.GetTimestamp()) - frameStart);
            frameStart = TicksToNanos(Stopwatch.GetTimestamp());
            if (!shouldContinue()) break;
            action();
            var frameEnd = TicksToNanos(Stopwatch.GetTimestamp());
            var took = frameEnd - frameStart;
            delayConsumer(took);
            var sleepTime = targetDelay - took;
            if (sleepTime > 0)
                Thread.Sleep(TimeSpan.FromMicroseconds(sleepTime/ 1000));
        }
    }   
}

