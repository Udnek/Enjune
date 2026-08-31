using System.Diagnostics;
using System.Reflection;
using Enjune.Attribute;

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
        var fields = objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var disposedAtLestOne = false;
        Logger.Info(typeof(Utils), $"{tab}- stared disposing {objType.Name}:");
        List<(FieldInfo Field, IDisposable Value, string Reason)> disposeAtLast = [];
        foreach (var field in fields)
        {
            var value = field.GetValue(obj);
            if (value is not IDisposable disposable) continue;
            var doNotSerialize = (DoNotDisposeViaUtilsAttribute?) field.GetCustomAttribute(typeof(DoNotDisposeViaUtilsAttribute));
            if (doNotSerialize is not null)
            {
                Logger.Highlight(typeof(Utils), $"  {tab}do not disposing {objType.Name}.{field.Name}: {doNotSerialize.Reason}");
                continue;
            }
            var isDisposeAtLast = (DisposeAtLastAttribute?) field.GetCustomAttribute(typeof(DisposeAtLastAttribute));
            if (isDisposeAtLast is not null)
            {
                disposeAtLast.Add((field, disposable, isDisposeAtLast.Reason));
                continue;
            }
            Dispose(field, disposable);
        }

        foreach (var (field, value, reason) in disposeAtLast)
        {
            Logger.Highlight(typeof(Utils), $"  {tab}disposing {objType.Name}.{field.Name} at last: {reason}");
            Dispose(field, value);
        }
        
        if (!disposedAtLestOne)
            Logger.Warn(typeof(Utils), $"  {tab}Nothing disposed in {objType.Name}. Something might be wrong");
        
        Logger.Info(typeof(Utils), $"{tab}- finished disposing {objType.Name}");
        
        _disposeDepth--;
        return;

        void Dispose(FieldInfo field, IDisposable value)
        {
            value.Dispose();
            disposedAtLestOne = true;
            Logger.Info(typeof(Utils),$"  {tab}{objType.Name}.{field.Name} disposed");
        }
    }
    
}

