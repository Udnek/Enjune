using System.Reflection;
using Enjune.Misc;

namespace SceneMaker;

internal class Program
{
    
    public static Assembly Assembly => typeof(Program).Assembly;
    
    private static void Main(string[] args)
    {
        Logger.RegisterNamespaceToDomain(typeof(Program).Assembly, "", new Logger.Domain("SceneMaker", ConsoleColor.DarkRed));
        // Enjune.Enjune.Run(new App(), args);
        Logger.Info(typeof(Program), "fucking niggas");
    }
}