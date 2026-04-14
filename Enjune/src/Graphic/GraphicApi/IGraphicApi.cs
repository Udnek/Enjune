using Enjune.File;
using Enjune.Graphic.Asset;
using Enjune.Graphic.InputHandler;
using OpenTK.Mathematics;

namespace Enjune.Graphic.GraphicApi;

public interface IGraphicApi : IDisposable
{
    Error? Init(CompiledAssets assets, int width, int height, string title, 
        IUserInputHandler userInputHandler, WindowSizeChangeHandler windowSizeHandler);
    
    void ViewPort(int x, int y, int width, int height);
    void Title(string title);
    // uniforms
    void ModelTransform(Matrix4 model);
    void ViewTransform(Matrix4 view);
    void ProjectionTransform(Matrix4 proj);
    void GlobalColor(Color color);
    // uniforms end
    
    // general pipeliner
    bool ShouldStop(); // should stop application
    void ClearScreenBuffers();
    void RenderToScreenBuffer(VertexBuffer buffer);
    void RenderPixelsToScreenBuffer(FixedBuffer<Vector2> pixelPosition);
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
    void SwitchShader(ShaderType type);
    
    enum ShaderType
    {
        Main,
        Text
    }
    
    delegate void WindowSizeChangeHandler(int width, int height);

    enum DrawMode
    {
        Fill,
        Wireframe
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