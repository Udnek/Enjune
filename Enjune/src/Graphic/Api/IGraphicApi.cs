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
        CreateDynamicRenderable<TPerVert, TPerPrim>(Mesh<TPerVert, TPerPrim>? initialMesh = null) where TPerVert : unmanaged where TPerPrim : unmanaged;

    /// <summary>
    /// Creates shader.
    /// Vertex shader must looks like:
    /// """
    /// vec4 process(Input input, out Varyings varyings){
    ///     input.vertex // returns TPerVert
    ///     input.perPrim // returns TPerPrim for current primitive
    /// 
    ///     uniformName.fieldName // access to uniform
    ///     uniformArrayName[i].fieldName // access to uniformArray
    /// 
    ///     varyings.color // access to varyings
    ///
    ///     return position;
    /// }
    /// """
    /// Fragment shader:
    /// """
    /// vec4 process(Varyings varyings, PerPrimitive perPrim){
    ///     perPrim // information for current primitive
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
    public ResultOrError<IShader<TPerVert, TPerPrim>>
        CreateShader<TPerVert, TPerPrim, TVaryings>(string vertexCode, string fragmentCode) where TPerVert : unmanaged where TPerPrim : unmanaged where TVaryings : unmanaged;
    
    /// <summary>
    /// Creates a uniform that can be accessed in any shader any time through it's name
    /// Struct must be aligned to std140
    /// </summary>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ResultOrError<IUniform<T>> 
        CreateUniform<T>(string name) where T : unmanaged;

    /// <summary>
    /// Creates an array that can be accessed in any shader any time through it's name
    /// Struct must be aligned to std430
    /// </summary>
    /// <param name="name"></param>
    /// <param name="initialCapacity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ResultOrError<IUniformArray<T>> 
        CreateUniformArray<T>(string name, int initialCapacity) where T : unmanaged;

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