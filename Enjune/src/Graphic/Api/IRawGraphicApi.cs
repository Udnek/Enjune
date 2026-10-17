using Enjune.Graphic.Asset;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Api;

public interface IRawGraphicApi
{
    IGraphicApi? Init(
        Vector2i windowSize,
        string title,
        IUserInputHandler inputHandler,
        out Error? error);
}