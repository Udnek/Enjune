namespace Enjune.Misc;

public static class Logger
{
    public static void Log(object author, object? msg) => Write(author, msg, "LOG");
    public static void Warn(object author, object? msg) => Write(author, msg, "WAR", ConsoleColor.Yellow);
    public static void Error(object author, object? msg) => Write(author, msg, "ERR", ConsoleColor.Red);
    public static void Highlight(object author, object? msg) => Write(author, msg, "HIL", ConsoleColor.Green);
    
    public enum Domain
    {
        FileSystem,
        Graphics,
        Physics,
    }
    
    public static void Log(Domain domain, object author, object? msg) => Write(author, msg, "LOG");
    public static void Warn(Domain domain, object author, object? msg) => Write(author, msg, "WARN", ConsoleColor.Yellow);
    public static void Error(Domain domain, object author, object? msg) => Write(author, msg, "ERR", ConsoleColor.Red);
    public static void Highlight(Domain domain, object author, object? msg) => Write(author, msg, "HIL", ConsoleColor.Green);
    
    private static void Write(object author, object? msg, string type, ConsoleColor? color = null)
    {
        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        string authorName = GetAuthorName(author);

        if (color is null)
            Print();
        else
        {
            var initialFrontColor = Console.ForegroundColor;
            Console.ForegroundColor = (ConsoleColor) color;
            Print();
            Console.ForegroundColor = initialFrontColor;
        }

        return;

        void Print() => Console.Write($"[{time}] [{type}] {authorName}: {msg ?? "null"}\n");
    }

    public static string GetAuthorName(object author)
    {
        if (author is string authorName) return authorName;
        Type authorType;
        if (author is Type at)
            authorType = at;
        else
            authorType = author.GetType();
        
        return GetTypeName(authorType);
    }
    
    public static string GetTypeName(Type author)
    {
        string prefix;
        if (author.DeclaringType != null)
            prefix = GetTypeName(author.DeclaringType) + ".";
        else 
            prefix = "";
        
        if (!author.IsGenericType) 
            return prefix + author.Name;
        var generics = author.GetGenericArguments();
        var name = author.Name;
        name = name[..name.IndexOf('`')];
        return $"{prefix}{name}{generics.Select(GetTypeName).ContentToString("<", ", ", ">")}";
    }
}