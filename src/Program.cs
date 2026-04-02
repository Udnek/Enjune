namespace Enjune;

internal static class Program
{
    public static void Main(string[] args)
    {
        var app = new App();
        app.Init();
        app.Run();

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}