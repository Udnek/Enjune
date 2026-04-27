using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;

namespace Enjune.Misc;

public delegate void Consumer<in T>(T obj);

public static class Utils
{
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

