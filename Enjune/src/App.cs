namespace Enjune;

public interface IApp
{
    public void Init(out string? error);
    public void Run();
}