using System.Reflection;
using Enjune.Misc;

namespace Enjune.Logger;

public static partial class Logger
{
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

    /// <summary>
    /// generates fancy-looking representation of type (including generic types)
    /// </summary>
    public static string GetTypeName<T>() => GetTypeName(typeof(T));
    
    /// <summary>
    /// generates fancy-looking representation of type (including generic types)
    /// </summary>
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