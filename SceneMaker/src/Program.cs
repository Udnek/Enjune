using Enjune.Ecs.EcsType;
using Enjune.Misc;
using OpenGLApi.Component.Buffer;

namespace SceneMaker;

class Program
{
    private static void Main(string[] args)
    {
        // Console.WriteLine(typeof(Archetype).Namespace);
        Logger.RegisterNamespaceToDomain(typeof(Program).Assembly, "", new Logger.Domain("SceneMaker", ConsoleColor.DarkRed));
        Enjune.Enjune.Run(new App(), args);
    }
}