using Enjune.Graphic.Input;
using OpenTK.Mathematics;

namespace Enjune.Graphic.GraphicApi;

public interface IUserInputHandler
{
    void HandleKey(KeyCode key, IGraphicApi.KeyAction action);
    void HandleCursor(int x, int y);
    void HandleScroll(float x, float y);
    void HandleWindowSizeChange(Vector2i newSize);
}