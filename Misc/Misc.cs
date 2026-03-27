using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace Enjune.Misc;

public delegate void Consumer<in T>(T obj);

public static class Misc
{
    extension(Matrix4 matrix)
    {
        public Vector3 TransformDirection(Vector3 vector) => Vector3.TransformVector(vector, matrix);
        public Vector3 TransformPosition(Vector3 vector) => Vector3.TransformPosition(vector, matrix);
    }
    
    public static Nanoseconds FpsToNanoDelay(Fps fps) => (Nanoseconds)(1_000_000_000.0 / fps);
    public static Fps NanoDelayToFps(Nanoseconds nanos) => (Fps)(1_000_000_000.0 / nanos);
    private static Nanoseconds TicksToNanos(long ticks) => (Nanoseconds)(ticks * (1_000_000_000.0 / Stopwatch.Frequency));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RunTargetFpsLoopWhile(
        Fps targetFps, Consumer<Nanoseconds> delayConsumer, Func<bool> shouldContinue, Action action)
    {
        var targetDelay = FpsToNanoDelay(targetFps);
        while (true)
        {
            var frameStart = TicksToNanos(Stopwatch.GetTimestamp());
            if (!shouldContinue()) break;
            action();
            var took = TicksToNanos(Stopwatch.GetTimestamp()) - frameStart;
            delayConsumer(took);
            var sleepTime = targetDelay - took;
            if (sleepTime > 0)
                Thread.Sleep(TimeSpan.FromMicroseconds(sleepTime/ 1000));
        }
    }   
}

