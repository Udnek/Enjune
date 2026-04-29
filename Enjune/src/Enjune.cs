using System.Reflection;
using Enjune.Misc;

namespace Enjune;

public static class Enjune
{
    public static Assembly Assembly => typeof(Enjune).Assembly;
    
    public static void Run(IApp app, string[] args)
    {
        var error = RunUntilError(app);
        if (error != null)
        {
            Logger.Error(typeof(Enjune), error);
        }
        app.Dispose();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private static Error? RunUntilError(IApp app)
    {
        Error? error;
        try
        {
            error = app.Init();
        }
        catch (Exception e)
        {
             return $"can not init app: {e}";
        }
        
        if (error != null) return $"can not init app: {error}";
        
        try
        {
            app.MainCycle();
        }
        catch (Exception e)
        {
            return $"error during app main cycle: {e}";
        }

        return null;
    }
}