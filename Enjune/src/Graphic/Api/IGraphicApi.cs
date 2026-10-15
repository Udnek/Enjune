using Enjune.File;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Api;

public interface IGraphicApi : IDisposable
{
    
    public ResultOrError<IRenderableMesh<TPerVert, TPerPrim>> 
        CreateRenderable<TPerVert, TPerPrim>(Mesh<TPerVert, TPerPrim> mesh) where TPerVert : unmanaged where TPerPrim : unmanaged;
    
    public ResultOrError<IRenderableMesh<TPerVert, TPerPrim>.IDynamic> 
        CreateDynamicRenderable<TPerVert, TPerPrim>(Mesh<TPerVert, TPerPrim>? mesh = null) where TPerVert : unmanaged where TPerPrim : unmanaged;

    /// <summary>
    /// Creates shader.
    /// Vertex shader must looks like:
    /// """
    /// void process(Input input, out Varyings varyings){
    ///     input.vertex // returns TPerVert
    ///     input.perPrim // returns TPerPrim for current primitive
    /// 
    ///     uniformName.fieldName // access to uniform
    ///     uniformArrayName[i].fieldName // access to uniformArray
    /// 
    ///     varyings.color // access to varyings
    /// }
    /// """
    /// Fragment shader:
    /// """
    /// vec4 process(Varyings varyings){
    ///     ...
    /// 
    ///     return color // func must return vec4 color of pixel
    /// }
    /// """
    /// </summary>
    /// <param name="vertexCode"></param>
    /// <param name="fragmentCode"></param>
    /// <typeparam name="TPerVert"></typeparam>
    /// <typeparam name="TPerPrim"></typeparam>
    /// <typeparam name="TVaryings"></typeparam>
    /// <returns></returns>
    public ResultOrError<IShader<TPerVert, TPerPrim, TVaryings>>
        CreateShader<TPerVert, TPerPrim, TVaryings>(string vertexCode, string fragmentCode) where TPerVert : unmanaged where TPerPrim : unmanaged;
    
    // ubo
    public ResultOrError<IUniform<T>> 
        CreateUniform<T>(string name) where T : unmanaged;
    
    // ssbo
    public ResultOrError<IUniformArray<T>> 
        CreateUniformArray<T>(string name) where T : unmanaged;

    // general pipeliner
    bool ShouldStop(); // should stop application
    void ClearRenderBuffer(bool color = true, bool depth = true);
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