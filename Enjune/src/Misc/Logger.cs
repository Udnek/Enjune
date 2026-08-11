using System.Reflection;

namespace Enjune.Misc;

public static class Logger
{
    public static void Info(object author, object? msg) => Info(null, author, msg);
    public static void Warn(object author, object? msg) => Warn(null, author, msg);
    public static void Error(object author, object? msg) => Error(null, author, msg);
    public static void Highlight(object author, object? msg) => Highlight(null, author, msg);

    public static void Info(Domain? domain, object author, object? msg)
    {
        if (IgnoreInfoLogs) return;
        Print(domain, author, msg, "INF");
    }
    public static void Warn(Domain? domain, object author, object? msg) => Print(domain, author, msg, "WAR", ConsoleColor.Yellow);
    public static void Error(Domain? domain, object author, object? msg) => Print(domain, author, msg, "ERR", ConsoleColor.Red);
    public static void Highlight(Domain? domain, object author, object? msg) => Print(domain, author, msg, "HIL", ConsoleColor.Green);

    public static bool IgnoreInfoLogs { get; set; } = false;

    // registering domain
    private static readonly Dictionary<Type, Domain> TypeToDomain = [];
    public static void RegisterTypeToDomain(Type type, Domain domain)
    {
        TypeToDomain[type] = domain;
        Info(Domain.Logger,typeof(Logger), $"registered domain [{domain.Name}] for type {type.Name}");
    }
    private static readonly List<(Assembly Assembly, string Namespace, Domain domain)> NamespaceToDomain = [];
    public static void RegisterNamespaceToDomain(Assembly assembly, string namespaceName, Domain domain)
    {
        NamespaceToDomain.Add((assembly, namespaceName, domain));
        NamespaceToDomain.Sort((left, right) =>
        {
            if (left.Assembly != right.Assembly) return 0;
            // e.g. Ecs.Dir is parent dir of Ecs => left is more prior
            if (left.Namespace.Contains(right.Namespace)) return -1;
            // opposite
            if (right.Namespace.Contains(left.Namespace)) return 1;
            return 0;
        });
        Info(Domain.Logger, typeof(Logger), $"registered domain [{domain.Name}] for assembly {assembly.GetName().Name} and namespace '{namespaceName}'");
    }
    // end registering domain

    private static Domain? GetRegisteredDomainFor(Type type)
    {
        if (TypeToDomain.TryGetValue(type, out var value))
            return value;
        var typeNs = type.Namespace;
        if (typeNs is null) return null;
        var typeAssembly = type.Assembly;
        foreach (var (assembly, ns, domain) in NamespaceToDomain)
        {
            if (typeAssembly != assembly) continue;
            if (!typeNs.Contains(ns)) continue;
            return domain;
        }

        return null;
    }
    
    public struct Domain(string name, ConsoleColor? color)
    {
        public static readonly Domain Default = new("Unknown", null);
        public static readonly Domain Enjune = new("Enjune", ConsoleColor.DarkGreen);
        public static readonly Domain Graphics = new("Graphics", ConsoleColor.Green);
        public static readonly Domain Logger = new("Logger", null);
        public static readonly Domain Assets = new("Assets", ConsoleColor.DarkMagenta);
        public static readonly Domain Misc = new("Misc", ConsoleColor.White);
        public static readonly Domain Ecs = new("ECS", ConsoleColor.DarkYellow);
        
        public readonly string Name = name;
        public readonly ConsoleColor? Color = color;
    }
    
    private static void Print(Domain? domain, object author, object? msg, string type, ConsoleColor? msgColor = null)
    {
        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        string authorName = GetAuthorName(author);
        domain ??= GetRegisteredDomainFor(author as Type ?? author.GetType()) ?? Domain.Default;

        PrintWithColor($"[{time}] ", null);
        PrintWithColor($"[{type}] ", msgColor);
        PrintWithColor($"[{domain.Value.Name}] ", domain.Value.Color);
        PrintWithColor($"{authorName}: {msg ?? "null"}\n", msgColor);
    }

    private static void PrintWithColor(string text, ConsoleColor? color)
    {
        if (color is null)
            Console.Write(text);
        else
        {
            var initialFrontColor = Console.ForegroundColor;
            Console.ForegroundColor = (ConsoleColor) color;
            Console.Write(text);
            Console.ForegroundColor = initialFrontColor;
        }
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

    public static string GetTypeName<T>() => GetTypeName(typeof(T));

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
        return $"{prefix}{name}{generics.Map(GetTypeName).ContentToString("<", ", ", ">")}";
    }
}