namespace Enjune;

public static class Logger
{
    public static void Log(object msg) => Write(msg, "LOG", ConsoleColor.Gray);
    public static void Warn(object msg) => Write(msg, "WARN", ConsoleColor.Yellow);
    public static void Error(object msg) => Write(msg, "ERR", ConsoleColor.Red);
    
    private static void Write(object msg, string type, ConsoleColor color)
    {
        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        var initFrontColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        
        Console.Write($"[{time}] [{type}] {msg}\n");
        Console.ForegroundColor = initFrontColor;
    }
}