namespace Enjune;

public static class Logger
{
    // public static void Log(object msg) => Write(msg, "LOG", ConsoleColor.Gray);
    // public static void Warn(object msg) => Write(msg, "WARN", ConsoleColor.Yellow);
    // public static void Error(object msg) => Write(msg, "ERR", ConsoleColor.Red);
    
    public static void Log(object author, object msg) => Write(author, msg, "LOG", ConsoleColor.Gray);
    public static void Warn(object author, object msg) => Write(author, msg, "WARN", ConsoleColor.Yellow);
    public static void Error(object author, object msg) => Write(author, msg, "ERR", ConsoleColor.Red);
    
    private static void Write(object author, object msg, string type, ConsoleColor color)
    {
        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        string authorName;
        if (author is Type typeType)
            authorName = typeType.Name;
        else
            authorName = author.GetType().Name;
        
        var initialFrontColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        
        Console.Write($"[{time}] [{type}] {authorName}: {msg}\n");
        Console.ForegroundColor = initialFrontColor;
    }
}