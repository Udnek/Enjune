using Enjune.File;
using Enjune.Graphic.Asset;
using Enjune.Graphic.GraphicApi.OpenGL;
using Enjune.Graphic.GraphicApi.Vertex.Colored;
using Enjune.Graphic.GraphicApi.Vertex.Material;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.GraphicApi;

public interface IGraphicApi : IDisposable
{
    delegate void WindowSizeChangeHandler(int width, int height);
    
    Error? Init(CompiledAssets assets, int width, int height, string title, int verticesCapacity,
        IUserInputHandler userInputHandler, WindowSizeChangeHandler windowSizeHandler);
    
    void Title(string title);
    
    // general pipeliner
    bool ShouldStop(); // should stop application
    void ClearScreenBuffers(bool color = true, bool depth = true);
    void UpdateScreen();
    void UpdateEvents(); // such as keyboard, mouse, etc
    // general pipeline end

    // misc
    void SetClearColor(Color color);
    void DumpTextures(ExternalPath path);
    void SetDrawMode(DrawMode mode);
    void SetCursorMode(CursorMode mode);
    void SetVsync(bool vsync);
    CursorMode GetCursorMode();
    Vector2i GetWindowSize();
    void SetWindowSize(Vector2i wh);
    
    // shader
    void UseShader<T>(Consumer<T> consumer) where T : IShader;

    enum Primitive
    {
        Triangle,
        LineStrip,
        LineLoop,
        Line,
        Point
    }
    
    enum DrawMode
    {
        Fill,
        Wireframe,
        Point
    }

    enum CursorMode
    {
        Normal,
        Invisible,
        Centered,
        CanNotLeaveWindow
    }
    
    enum KeyAction
    {
        Press,
        Release,
        Repeat
    }
}