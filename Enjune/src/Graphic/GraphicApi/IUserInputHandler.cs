using Enjune.Graphic.Input;

namespace Enjune.Graphic.GraphicApi;

public interface IUserInputHandler
{
    void HandleKey(KeyCode key, IGraphicApi.KeyAction action);
    void HandleCursor(int x, int y);
}