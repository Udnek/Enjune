using Enjune.Graphic.Key;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Api;

public interface IUserInputHandler
{
    void HandleKey(KeyCode key, IGraphicApi.KeyAction action);
    void HandleCharacter(char character);
    void HandleCursorFromLeftBottom(int x, int y); // (0; 0) at left bottom
    void HandleScroll(float x, float y);
    void HandleWindowSizeChange(Vector2i newSize);
}