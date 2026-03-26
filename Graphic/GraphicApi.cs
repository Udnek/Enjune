using System.Numerics;
using Engine.Graphic.KeyHandler;

namespace Engine.Graphic;

public interface GraphicApi
{
    void Init(int width, int height, string title, UserInputHandler userInputHandler, WindowSizeChangeHandler windowSizeHandler);
    void ViewPort(int x, int y, int width, int height);
    void Title(string title);
    // uniforms
    void Model(Matrix4x4 model);
    void View(Matrix4x4 view);
    void Projection(Matrix4x4 proj);
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