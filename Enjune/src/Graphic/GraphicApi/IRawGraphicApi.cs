using Enjune.Graphic.Asset;
using OpenTK.Mathematics;

namespace Enjune.Graphic.GraphicApi;

public interface IRawGraphicApi
{
    IGraphicApi? Init(CompiledAssets assets, Vector2i windowSize, string title, IUserInputHandler inputHandler, out Error? error);
}