using System.Reflection;
using Enjune.Data.Codec;
using Enjune.Data.Json;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using Enjune.Registering;
using SceneMaker.Ecs.Component;
using SceneMaker.Misc;

namespace SceneMaker;

internal class Program
{
    
    public static Assembly Assembly => typeof(Program).Assembly;
    
    private static void Main(string[] args)
    {
        Logger.RegisterNamespaceToDomain(typeof(Program).Assembly, "", new Logger.Domain("SceneMaker", ConsoleColor.DarkRed));
        Enjune.Enjune.Run(new App(), args);
    }
}