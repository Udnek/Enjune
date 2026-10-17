using Enjune.File;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Api;

public interface IGraphicApi : IDisposable
{
    /// <summary>
    /// Creates a slot for any texture, that can be later bind and sampled in shader
    /// </summary>
    /// <typeparam name="TexType"></typeparam>
    /// <returns></returns>
    public ResultOrError<ITextureSlot<TexType>> CreateTextureSlot<TexType>(string inShaderName) where TexType : ITexture;
    
    /// <summary>
    /// Creates sample-only texture from byte image
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public ResultOrError<ITexture> CreateTexture(ByteImage image);
    
    /// <summary>
    /// Creates an array of textures from array of byte images.
    /// Note: all images must be of the same size
    /// </summary>
    /// <param name="images"></param>
    /// <returns></returns>
    public ResultOrError<ITextureArray> CreateTextureArray(ReadOnlySpan<ByteImage> images);

    /// <summary>
    /// Creates an empty texture that can be used as sample or be bind as render target
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public ResultOrError<IDynamicTexture> CreateDynamicTexture(Vector2i size);
    
    /// <summary>
    /// Creates a static mesh that can be rendered in appropriate shader
    /// </summary>
    /// <param name="mesh"></param>
    /// <typeparam name="TPerVert"></typeparam>
    /// <typeparam name="TPerPrim"></typeparam>
    /// <returns></returns>
    public ResultOrError<IRenderableMesh<TPerVert, TPerPrim>> 
        CreateRenderableMesh<TPerVert, TPerPrim>(Mesh<TPerVert, TPerPrim> mesh) where TPerVert : unmanaged where TPerPrim : unmanaged;

    /// <summary>
    /// Creates a dynamic mesh that can be rendered in appropriate shader and can be refit
    /// </summary>
    /// <param name="initialMesh"></param>
    /// <typeparam name="TPerVert"></typeparam>
    /// <typeparam name="TPerPrim"></typeparam>
    /// <returns></returns>
    public ResultOrError<IRenderableMesh<TPerVert, TPerPrim>.IDynamic> 
        CreateDynamicRenderableMesh<TPerVert, TPerPrim>(Mesh<TPerVert, TPerPrim>? initialMesh = null) where TPerVert : unmanaged where TPerPrim : unmanaged;

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
    /// Creates a uniform that can be accessed in any shader any time through its name
    /// Struct must be aligned to std140
    /// </summary>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ResultOrError<IUniform<T>> 
        CreateUniform<T>(string name) where T : unmanaged;

    /// <summary>
    /// Creates an array that can be accessed in any shader any time through its name
    /// Struct must be aligned to std430
    /// </summary>
    /// <param name="name"></param>
    /// <param name="initialCapacity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ResultOrError<IUniformArray<T>> 
        CreateUniformArray<T>(string name, int initialCapacity) where T : unmanaged;

    // general pipeliner
    void ClearCurrentRenderTarget(bool color = true, bool depth = true);
    void BindScreenRenderTarget();
    void UpdateScreen();
    void UpdateEvents(); // such as keyboard, mouse, etc
    // general pipeline end

    // misc ;
    void SetClearColor(Color color);
    void SetDrawMode(DrawMode mode);
    void SetCursorMode(CursorMode mode);
    CursorMode GetCursorMode();
    void SetVsync(bool vsync);
    Vector2i GetWindowSize();
    void SetWindowSize(Vector2i size);
    void SetTitle(string title);
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