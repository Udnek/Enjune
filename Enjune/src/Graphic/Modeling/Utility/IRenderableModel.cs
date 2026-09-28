using Enjune.Graphic.Api;

namespace Enjune.Graphic.Modeling.Utility;

public interface IRenderableModel : IDisposable
{
    public void Render(IShader shader);
}