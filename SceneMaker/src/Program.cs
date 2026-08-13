using System.Reflection;
using Enjune.Data.Codec;
using Enjune.Data.Json;
using Enjune.Misc;
using Enjune.Registering;

namespace SceneMaker;

internal class Program
{
    
    public static Assembly Assembly => typeof(Program).Assembly;
    
    private static void Main(string[] args)
    {
        Logger.RegisterNamespaceToDomain(typeof(Program).Assembly, "", new Logger.Domain("SceneMaker", ConsoleColor.DarkRed));
        Enjune.Enjune.Run(new EcsApp(), args);
    }
}