using Enjune.Graphic.Api;

namespace Enjune.Graphic.Modeling.Utility;

public interface IRenderableModel
{
    public void Render(IShader shader);
}