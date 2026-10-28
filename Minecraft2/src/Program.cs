using System.Reflection;
using Enjune.Logger;
using Enjune.Misc;

namespace Minecraft2;

public class Program
{
    internal static readonly Assembly Assembly = typeof(Program).Assembly;
    
    internal static void Main(string[] args)
    {
        Logger.RegisterNamespaceToDomain(Assembly, "", new Logger.Domain("MC2", ConsoleColor.Yellow));
        Logger.IgnoreDebugLogs = true;
        Enjune.Enjune.Run(new App(), args);
    }
}