using Enjune.Graphic.KeyHandler;
using OpenTK.Mathematics;

namespace Enjune.Graphic;

public interface IGraphicApi
{
    void Init(int width, int height, string title, IUserInputHandler userInputHandler, WindowSizeChangeHandler windowSizeHandler);
    void ViewPort(int x, int y, int width, int height);
    void Title(string title);
    // uniforms
    void Model(Matrix4 model);
    void View(Matrix4 view);
    void Projection(Matrix4 proj);
    // uniforms end
    void PutVertex(Position position, Color color);
    void ClearRenderBuffer();
    void Render();
    void UpdateEvents();
    bool ShouldStop();
    void Destroy();
    void ClearColor(Color color);

    delegate void WindowSizeChangeHandler(int width, int height);

    enum KeyAction
    {
        Press,
        Release,
        Repeat
    }
}