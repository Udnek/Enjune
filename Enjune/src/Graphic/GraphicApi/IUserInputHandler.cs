using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Enjune.Graphic.GraphicApi;

public interface IUserInputHandler
{
    void HandleKey(GlfwKey key, IGraphicApi.KeyAction action);
    void HandleMouseKey(MouseButton key, IGraphicApi.KeyAction action);
    void HandleCursor(int x, int y);
}