using System.Reflection;
using Enjune.Misc;

namespace Enjune;

public static class Enjune
{
    public static Assembly Assembly => typeof(Enjune).Assembly;
    
    public static void Run(IApp app, string[] args)
    {
        app.Init(out string? error);
        if (error != null)
        {
            error = $"can not init app: {error}";
            Logger.Error(typeof(Enjune), error);
            throw new Exception(error);
        }
        app.Run();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}