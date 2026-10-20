using System.Reflection;
using Enjune.Misc;

namespace ExampleApp;

class Program
{
    internal static readonly Assembly Assembly = typeof(Program).Assembly;
    
    internal static void Main(string[] args)
    {
        Logger.RegisterNamespaceToDomain(Assembly, "", new Logger.Domain("EXM", ConsoleColor.Yellow));
        Enjune.Enjune.Run(new App(), args);
    }
}