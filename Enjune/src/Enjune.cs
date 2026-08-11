using System.Reflection;
using Enjune.Misc;

namespace Enjune;

public static class Enjune
{
    public static Assembly Assembly => typeof(Enjune).Assembly;

    static Enjune()
    {
        Logger.RegisterNamespaceToDomain(Assembly, nameof(Graphic.Asset), Logger.Domain.Assets);
        Logger.RegisterNamespaceToDomain(Assembly, nameof(Ecs), Logger.Domain.Ecs);
        Logger.RegisterNamespaceToDomain(Assembly, nameof(Misc), Logger.Domain.Misc);
    }
    
    public static void Run(IApp app, string[] args)
    {
        var error = RunUntilError(app);
        if (error != null)
        {
            Logger.Error(typeof(Enjune), error);
        }
        app.Dispose();
        
        // to catch forgotten disposables
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
            error = e.ToString();
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