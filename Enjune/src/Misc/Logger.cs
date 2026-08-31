using System.Reflection;
using System.Runtime.CompilerServices;
using Enjune.Attribute;

namespace Enjune.Misc;

public static class Logger
{
    static Logger()
    {
        RegisterTypeToDomain(typeof(Logger), Domain.Logger);
    }
    
    public static void Info(object author, object? msg, [CallerMemberName] string member = "") => Info(null, author, msg, member);
    public static void Warn(object author, object? msg, [CallerMemberName] string member = "") => Warn(null, author, msg, member);
    public static void Error(object author, object? msg, [CallerMemberName] string member = "") => Error(null, author, msg, member);
    public static void Highlight(object author, object? msg, [CallerMemberName] string member = "") => Highlight(null, author, msg, member);

    public static void Info(Domain? domain, object author, object? msg, [CallerMemberName] string member = "")
    {
        if (IgnoreInfoLogs) return;
        Print(domain, author, member, msg, "INF");
    }
    public static void Warn(Domain? domain, object author, object? msg, [CallerMemberName] string member = "") => Print(domain, author, member, msg, "WRN", ConsoleColor.Yellow);
    public static void Error(Domain? domain, object author, object? msg, [CallerMemberName] string member = "") => Print(domain, author, member, msg, "ERR", ConsoleColor.Red);
    public static void Highlight(Domain? domain, object author, object? msg, [CallerMemberName] string member = "") => Print(domain, author, member, msg, "HIL", ConsoleColor.Green);

    public static bool IgnoreInfoLogs { get; set; } = false;

    // registering domain
    private static readonly Dictionary<Type, Domain> TypeToDomain = [];
    public static void RegisterTypeToDomain(Type type, Domain domain)
    {
        TypeToDomain[type] = domain;
        Info(typeof(Logger), $"registered domain [{domain.Name}] for type {type.Name}");
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
        Info(typeof(Logger), $"registered domain [{domain.Name}] for assembly {assembly.GetName().Name} and namespace '{namespaceName}'");
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
    
    private static void Print(Domain? domain, object author, string member, object? msg, string severityType, ConsoleColor? msgColor = null)
    {
        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        var logParams = GetLogParams(author);
        string authorName = GetAuthorName(author, logParams?.Mtd ?? LogParamsAttribute.Method.FancyTypeToString);
        domain ??= GetRegisteredDomainFor(author as Type ?? author.GetType()) ?? Domain.Default;

        PrintWithColor($"[{time}] ", null);
        PrintWithColor($"[{severityType}] ", msgColor);
        PrintWithColor($"[{domain.Value.Name}] ", domain.Value.Color);
        if (logParams?.LogCallingMethod ?? false)
            PrintWithColor($"{authorName}.{member}: {msg ?? "null"}\n", msgColor);
        else
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

    private static LogParamsAttribute? GetLogParams(object author)
    {
        Type authorType;
        if (author is Type at)
            authorType = at;
        else
            authorType = author.GetType();

        return authorType.GetCustomAttribute(typeof(LogParamsAttribute)) as LogParamsAttribute;
    }

    private static string GetAuthorName(object author, LogParamsAttribute.Method method)
    {
        switch (method)
        {
            case LogParamsAttribute.Method.ToString:
                return author.ToString() ?? "null";
            case LogParamsAttribute.Method.FancyTypeToString:
            {
                if (author is string authorName) 
                    return authorName;
        
                if (author is Type at)
                    return GetTypeName(at);
                
                return GetTypeName(author.GetType());
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }
    }

    // generates fancy-looking representation of type (including generic types)
    public static string GetTypeName<T>() => GetTypeName(typeof(T));
    
    // generates fancy-looking representation of type (including generic types)
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