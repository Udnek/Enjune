namespace Enjune;

public interface IApp : IDisposable
{
    public Error? Init();
    public void Run();
}