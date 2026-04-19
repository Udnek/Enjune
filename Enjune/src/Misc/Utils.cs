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

    public static Vector3 ToTk(this System.Numerics.Vector3 vector) => new(vector.X, vector.Y, vector.Z);
    
    public static string ContentToString<T>(this IEnumerable<T> array)
        => "["+string.Join(", ", array.Select(v => v?.ToString() ?? "null"))+"]";

    private static readonly float NanosInSec = 1_000_000_000.0f;
    public static Nanoseconds FpsToNanoDelay(Fps fps) => (Nanoseconds)(NanosInSec / fps);
    public static float NanosToSeconds(Nanoseconds nanos) => nanos / NanosInSec;
    public static Fps NanoDelayToFps(Nanoseconds nanos) => NanosInSec / nanos;
    private static Nanoseconds TicksToNanos(long ticks) => (Nanoseconds)(ticks * (NanosInSec / Stopwatch.Frequency));


    
    public static void RunTargetFpsLoopWhile(
        Fps targetFps, out float deltaTime, Consumer<Nanoseconds> delayConsumer, Func<bool> shouldContinue, Action action)
    {
        var targetDelay = FpsToNanoDelay(targetFps);
        
        var frameEnd = TicksToNanos(Stopwatch.GetTimestamp());
        while (true)
        { 
            var frameStart = TicksToNanos(Stopwatch.GetTimestamp());
            deltaTime = NanosToSeconds(frameStart - frameEnd);
            if (!shouldContinue()) break;
            action();
            frameEnd = TicksToNanos(Stopwatch.GetTimestamp());
            var took = frameEnd - frameStart;
            delayConsumer(took);
            var sleepTime = targetDelay - took;
            if (sleepTime > 0)
                Thread.Sleep(TimeSpan.FromMicroseconds(sleepTime/ 1000));
        }
    }   
}

