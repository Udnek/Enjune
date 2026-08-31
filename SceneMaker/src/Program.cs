using System.Reflection;
using Enjune.Misc;

namespace SceneMaker;

internal class Program
{
    
    public static Assembly Assembly => typeof(Program).Assembly;

    static Program()
    {
        Logger.RegisterNamespaceToDomain(typeof(Program).Assembly, "", new Logger.Domain("SceneMaker", ConsoleColor.DarkRed));
    }
    
    private static void Main(string[] args)
    {
        Enjune.Enjune.Run(new App(), args);
    }
}