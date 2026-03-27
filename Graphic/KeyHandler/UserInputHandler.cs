namespace Enjune.Graphic.KeyHandler;

public interface IUserInputHandler
{
    void Handle(GlfwKey key, IGraphicApi.KeyAction action);
}