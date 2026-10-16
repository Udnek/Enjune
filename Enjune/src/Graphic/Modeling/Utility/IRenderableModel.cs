using Enjune.Graphic.Api;

namespace Enjune.Graphic.Modeling.Utility;

public interface IRenderableModel<TPerVert, TPerPrim> where TPerVert : unmanaged where TPerPrim : unmanaged
{
    public void Render(IShader<TPerVert, TPerPrim> shader);
}