namespace Engine.Graphic.KeyHandler;

public interface IUserInputHandler
{
    void Handle(GlfwKey key, IGraphicApi.KeyAction action);
}