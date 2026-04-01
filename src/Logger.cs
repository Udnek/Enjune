namespace Enjune;

public static class Logger
{
    public static void Log(object author, object msg) => Write(author, msg, "LOG", null);
    public static void Warn(object author, object msg) => Write(author, msg, "WARN", ConsoleColor.Yellow);
    public static void Error(object author, object msg) => Write(author, msg, "ERR", ConsoleColor.Red);
    
    private static void Write(object author, object msg, string type, ConsoleColor? color)
    {
        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        string authorName;
        if (author is Type authorType)
            authorName = authorType.Name;
        else
            authorName = author.GetType().Name;
        
        var initialFrontColor = Console.ForegroundColor;
        if (color != null)
            Console.ForegroundColor = (ConsoleColor) color;
        
        Console.Write($"[{time}] [{type}] {authorName}: {msg}\n");
        Console.ForegroundColor = initialFrontColor;
    }
}