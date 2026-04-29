namespace Enjune.Graphic.GraphicApi;

public interface IRenderableModel<in T> : IDisposable where T : IShader
{
    public void Render(T shader);
}