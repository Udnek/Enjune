using Enjune.Graphic.GraphicApi;

namespace Enjune.Graphic.InputHandler;

public interface IUserInputHandler
{
    void Handle(GlfwKey key, IGraphicApi.KeyAction action);
}