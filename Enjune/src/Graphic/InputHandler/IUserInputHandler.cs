using Enjune.Graphic.GraphicApi;

namespace Enjune.Graphic.InputHandler;

public interface IUserInputHandler
{
    void HandleKey(GlfwKey key, IGraphicApi.KeyAction action);
    void HandleCursor(double x, double y);
}