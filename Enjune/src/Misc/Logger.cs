namespace Enjune.Misc;

public static class Logger
{
    public static void Log(object author, object? msg) => Write(author, msg, "LOG");
    public static void Warn(object author, object? msg) => Write(author, msg, "WARN", ConsoleColor.Yellow);
    public static void Error(object author, object? msg) => Write(author, msg, "ERR", ConsoleColor.Red);
    public static void Highlight(object author, object? msg) => Write(author, msg, "HL", ConsoleColor.Green);
    
    private static void Write(object author, object? msg, string type, ConsoleColor? color = null)
    {
        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        string authorName;
        if (author is Type authorType)
            authorName = authorType.Name;
        else if (author is string authorString)
            authorName = authorString;
        else
            authorName = author.GetType().Name;
        
        var initialFrontColor = Console.ForegroundColor;
        if (color != null)
            Console.ForegroundColor = (ConsoleColor) color;
        
        Console.Write($"[{time}] [{type}] {authorName}: {msg ?? "null"}\n");
        Console.ForegroundColor = initialFrontColor;
    }
}