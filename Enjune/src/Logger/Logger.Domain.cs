using System.Reflection;

namespace Enjune.Logger;

public static partial class Logger
{
    private static readonly Dictionary<Type, Domain> TypeToDomain = [];
    private static readonly List<(Assembly Assembly, string Namespace, Domain domain)> NamespaceToDomain = [];
    private static readonly HashSet<Domain> EnabledDebugDomains = [];
    
    static Logger()
    {
        RegisterNamespaceToDomain(Enjune.Assembly, nameof(Logger), Domain.Logger);
    }
    
    // registering domain
    public static void RegisterTypeToDomain(Type type, Domain domain)
    {
        TypeToDomain[type] = domain;
        Info(typeof(Logger), $"Registered domain [{domain.Name}] for type {GetTypeName(type)}");
    }
    
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
        Info(typeof(Logger), $"Registered domain [{domain.Name}] for assembly {assembly.GetName().Name} and namespace '{namespaceName}'");
    }
    // end registering domain

    public static void EnableDebugFor(Domain domain) => EnabledDebugDomains.Add(domain);

    public static void DisableDebugFor(Domain domain) => EnabledDebugDomains.Remove(domain);

    private static Domain? GetRegisteredDomainFor(Type type)
    {
        if (type.IsGenericType) // is List<Something> for example
            type = type.GetGenericTypeDefinition(); // returns List<>
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
    
    public readonly record struct Domain(string Name, ConsoleColor? Color)
    {
        public static readonly Domain Default = new("Unknown", null);
        public static readonly Domain Enjune = new("Enjune", ConsoleColor.DarkGreen);
        public static readonly Domain Graphics = new("Graphics", ConsoleColor.Green);
        public static readonly Domain Logger = new("Logger", null);
        public static readonly Domain Assets = new("Assets", ConsoleColor.DarkMagenta);
        public static readonly Domain Misc = new("Misc", ConsoleColor.White);
        public static readonly Domain Ecs = new("ECS", ConsoleColor.DarkYellow);
    }
}