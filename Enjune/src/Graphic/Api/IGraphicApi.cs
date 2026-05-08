using Enjune.File;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Api;

public interface IGraphicApi : IDisposable
{
    public IRenderableModel CreateStaticRenderable(Model model, Primitive primitive = Primitive.Triangle);
    public IRenderableModel.IDynamic CreateDynamicRenderable(Model model, Primitive primitive = Primitive.Triangle);
    
    void SetLights(IEnumerable<SpotLight> lights);

    // // TODO: Simplify this shit?
    // public void SetCurrentThreadToRender();
    
    // general pipeliner
    bool ShouldStop(); // should stop application
    void ClearRenderBuffer(bool color = true, bool depth = true);
    void UseShader<T>(Consumer<T> consumer) where T : IShader;
    void UpdateScreen();
    void UpdateEvents(); // such as keyboard, mouse, etc
    // general pipeline end

    // misc
    void SetRenderSize(Vector2i size);
    void SetClearColor(Color color);
    void DumpTextures(ExternalPath path);
    void SetDrawMode(DrawMode mode);
    void SetCursorMode(CursorMode mode);
    void SetVsync(bool vsync);
    CursorMode GetCursorMode();
    Vector2i GetWindowSize();
    void SetWindowSize(Vector2i size);
    void Title(string title);
    // misc end

    public static int PrimitivesAmountFromIndexes(Primitive primitive, int indexes)
    {
        return primitive switch
        {
            Primitive.Triangle => indexes / 3,
            Primitive.LineStrip => indexes - 1,
            Primitive.LineLoop => indexes,
            Primitive.Line => indexes / 2,
            Primitive.Point => indexes,
            _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null)
        };
    }
    
    public static int IndexStridePerPrimitive(Primitive primitive)
    {
        return primitive switch
        {
            Primitive.Triangle => 3,
            Primitive.LineStrip => 1,
            Primitive.LineLoop => 1,
            Primitive.Line => 2,
            Primitive.Point => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null)
        };
    }
    
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