using System.Reflection;
using Enjune.Misc;

namespace Enjune;

public static class Enjune
{
    public static Assembly Assembly => typeof(Enjune).Assembly;
    
    public static void Run(IApp app, string[] args)
    {
        try
        {
            var error = app.Init();
            if (error != null)
            {
                error = $"can not init app: {error}";
                Logger.Error(typeof(Enjune), error);
                throw new Exception(error);
            }

            app.Run();
        }
        finally
        {
            app.Dispose();
        }
    }
}