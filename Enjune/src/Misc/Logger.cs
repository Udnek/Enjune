namespace Enjune.Misc;

public static class Logger
{
    public static void Log(object author, object? msg) => Write(author, msg, "LOG");
    public static void Warn(object author, object? msg) => Write(author, msg, "WARN", ConsoleColor.Yellow);
    public static void Error(object author, object? msg) => Write(author, msg, "ERR", ConsoleColor.Red);
    public static void Highlight(object author, object? msg) => Write(author, msg, "HL", ConsoleColor.Green);
    
    public enum Domain
    {
        FileSystem,
        Graphics,
        Physics,
    }
    
    public static void Log(Domain domain, object author, object? msg) => Write(author, msg, "LOG");
    public static void Warn(Domain domain, object author, object? msg) => Write(author, msg, "WARN", ConsoleColor.Yellow);
    public static void Error(Domain domain, object author, object? msg) => Write(author, msg, "ERR", ConsoleColor.Red);
    public static void Highlight(Domain domain, object author, object? msg) => Write(author, msg, "HL", ConsoleColor.Green);
    
    private static void Write(object author, object? msg, string type, ConsoleColor? color = null)
    {
        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        string authorName = GetAuthorName(author);

        void Print() => Console.Write($"[{time}] [{type}] {authorName}: {msg ?? "null"}\n");

        if (color is null)
            Print();
        else
        {
            var initialFrontColor = Console.ForegroundColor;
            Console.ForegroundColor = (ConsoleColor) color;
            Print();
            Console.ForegroundColor = initialFrontColor;
        }
    }

    private static string GetAuthorName(object author)
    {
        if (author is string authorName) return authorName;
        Type authorType;
        if (author is Type at)
            authorType = at;
        else
            authorType = author.GetType();


        if (!authorType.IsGenericType) return authorType.Name;
        var generics = authorType.GetGenericArguments();
        return $"{authorType.Name}{generics.Select(g => g.Name).ContentToString("<", ", ", ">")}";
    }
}