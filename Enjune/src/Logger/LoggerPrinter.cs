using Enjune.Misc;
using Enjune.Parallelism;

namespace Enjune.Logger;

public readonly record struct PrinterJob(
    Logger.Domain Domain,
    string AuthorName,
    string Member,
    bool LogCallingMember,
    object? Message,
    string SeverityType,
    ConsoleColor? MessageColor);

public class LoggerPrinter : ConcurrentWorker<uint, PrinterJob, Nothing>
{
    protected override Nothing Work(PrinterJob job)
    {
        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        
        PrintWithColor($"[{time}] ", null);
        PrintWithColor($"[{job.SeverityType}] ", job.MessageColor);
        PrintWithColor($"[{job.Domain.Name}] ", job.Domain.Color);
        if (job.LogCallingMember)
            PrintWithColor($"{job.AuthorName}.{job.Member}: {job.Message ?? "null"}\n", job.MessageColor);
        else
            PrintWithColor($"{job.AuthorName}: {job.Message ?? "null"}\n", job.MessageColor);

        return Nothing.Instance;
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
}