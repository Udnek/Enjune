using Enjune.Graphic.Api;

namespace Enjune.KitStart.Modeling;

public interface IRenderableModel<TPerVert, TPerPrim> where TPerVert : unmanaged where TPerPrim : unmanaged
{
    public void Render(IShader<TPerVert, TPerPrim> shader);
}