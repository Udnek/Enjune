using Enjune.File;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Api;

public interface IGraphicApi : IDisposable
{
    public IRenderableMesh CreateStaticRenderable(MeshInstance mesh);
    public IRenderableMesh.IDynamic CreateDynamicRenderable(MeshInstance mesh);
    
    void SetLights(IEnumerable<SpotLight> lights);
    
    // general pipeliner
    bool ShouldStop(); // should stop application
    void ClearRenderBuffer(bool color = true, bool depth = true);
    void UseShader<T>(Consumer<T> consumer) where T : IShader;
    void UpdateScreen();
    void UpdateEvents(); // such as keyboard, mouse, etc
    // general pipeline end

    // misc
    Vector2i GetCursorPosition();
    void SetRenderSize(Vector2i size);
    void SetClearColor(Color color);
    void DumpTextures(ExternalPath path); // TODO fix
    void SetDrawMode(DrawMode mode);
    void SetCursorMode(CursorMode mode);
    void SetVsync(bool vsync);
    CursorMode GetCursorMode();
    Vector2i GetWindowSize();
    void SetWindowSize(Vector2i size);
    void Title(string title);
    // misc end
    
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