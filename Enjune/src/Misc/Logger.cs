using System.Reflection;
using System.Runtime.CompilerServices;
using Enjune.Attribute;

namespace Enjune.Misc;

public static partial class Logger
{
    static Logger()
    {
        RegisterTypeToDomain(typeof(Logger), Domain.Logger);
    }
    
    private static readonly ConcurrentWorker<uint, PrinterJob, Nothing> PrintWorker = new LoggerPrinter();
    private static uint _messageCounter = 0;
    
    public static bool IgnoreInfoLogs { get; set; } = false;
    
    public static void Info(object author, object? msg, [CallerMemberName] string member = "")
    {
        if (IgnoreInfoLogs) return;
        Print(author, member, msg, "INF");
    }

    public static void Warn(object author, object? msg, [CallerMemberName] string member = "") 
        => Print(author, member, msg, "WRN", ConsoleColor.Yellow);
    
    public static void Error(object author, object? msg, [CallerMemberName] string member = "") 
        => Print(author, member, $"{msg}; stacktrace: \n{Environment.StackTrace}", "ERR", ConsoleColor.Red);
    
    public static void Highlight(object author, object? msg, [CallerMemberName] string member = "") 
        => Print(author, member, msg, "HIL", ConsoleColor.Green);
    
    private static void Print(object author, string member, object? msg, string severityType, ConsoleColor? msgColor = null)
    {
        var logParams = GetLogParams(author);
        
        if (!PrintWorker.HasStated) 
            PrintWorker.Start("LoggerPrinter");

        var msgId = Interlocked.Add(ref _messageCounter, 1);
        PrintWorker.Enqueue(msgId, new PrinterJob
        {
            Domain = GetRegisteredDomainFor(author as Type ?? author.GetType()) ?? Domain.Default,
            AuthorName = GetAuthorName(author, logParams?.Mtd ?? LogParamsAttribute.Method.FancyTypeToString),
            Member = member,
            LogCallingMember = logParams?.LogCallingMethod ?? false,
            Message = msg,
            SeverityType = severityType,
            MessageColor = msgColor
        });
        
        while (PrintWorker.TryDequeue(out uint id, out var output))
        {
            // just dequeuing
        }
    }
}