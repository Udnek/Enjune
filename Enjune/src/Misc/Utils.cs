using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Mathematics;

namespace Enjune.Misc;

public delegate void Consumer<in T>(T obj);

public static class Utils
{
    private static readonly Nanoseconds NanosInSec = 1_000_000_000;
    public static Nanoseconds FpsToNanoDelay(Fps fps) => (Nanoseconds)(NanosInSec / fps);
    public static Seconds NanosToSeconds(Nanoseconds nanos) =>  nanos / (Seconds) NanosInSec;
    public static Fps NanoDelayToFps(Nanoseconds nanos) => NanosInSec / (Fps) nanos;
    private static Nanoseconds TicksToNanos(long ticks) => ticks * (NanosInSec / Stopwatch.Frequency);
    
    public static void RunTargetFpsLoopWhile(
        Fps targetFps, Func<bool> shouldContinue, Action<Seconds> action)
    {
        var targetDelay = FpsToNanoDelay(targetFps);
        
        var frameStart = TicksToNanos(Stopwatch.GetTimestamp());
        while (true)
        { 
            if (!shouldContinue()) break;
            var deltaTime = NanosToSeconds(TicksToNanos(Stopwatch.GetTimestamp()) - frameStart);
            frameStart = TicksToNanos(Stopwatch.GetTimestamp());
            action(deltaTime);
            var frameEnd = TicksToNanos(Stopwatch.GetTimestamp());
            Nanoseconds took = frameEnd - frameStart;
            Nanoseconds sleepTime = targetDelay - took;
            if (sleepTime > 0)
                Thread.Sleep(TimeSpan.FromMicroseconds(sleepTime / 1000));
        }
    }
    
    private static int _disposeDepth = 0;
    public static void DisposeAllFields(object obj)
    {
        var tab = new string(' ', _disposeDepth*2);
        _disposeDepth++;
        
        var objType = obj.GetType();
        var fields = objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic| BindingFlags.Instance);
        bool disposedAtLestOne = false;
        Logger.Log(typeof(Utils), $"{tab}- stared disposing {objType.Name}:");

        foreach (var field in fields)
        {
            var value = field.GetValue(obj);
            if (value is not IDisposable disposable) continue;
            disposable.Dispose();
            disposedAtLestOne = true;
            Logger.Log(typeof(Utils),$"  {tab}{objType.Name}.{field.Name} disposed");
        }
        if (!disposedAtLestOne)
            Logger.Warn(typeof(Utils), $"  {tab}Nothing disposed in {objType.Name}. Something might be wrong");
        
        Logger.Log(typeof(Utils), $"{tab}- finished disposing {objType.Name}");
        
        _disposeDepth--;
    }
    
}

