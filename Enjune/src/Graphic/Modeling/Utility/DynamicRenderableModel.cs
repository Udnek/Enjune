using Enjune.Graphic.Api;
using Enjune.Misc;

namespace Enjune.Graphic.Modeling.Utility;

/// <summary>
/// Basically:
/// - Create
/// - while you need:
///     - Clear
///     - Modify meshes
///     - Call refit
///     - Render
/// </summary>
public class DynamicRenderableModel<TPerVert, TPerPrim> : IRenderableModel<TPerVert, TPerPrim> where TPerVert : unmanaged where TPerPrim : unmanaged
{
    private readonly IGraphicApi _graphicApi;
    private readonly Dictionary<PrimitiveTopology, Mesh<TPerVert, TPerPrim>> 
        _meshes = new(Enum.GetValues<PrimitiveTopology>().Length);
    private readonly Dictionary<PrimitiveTopology, IRenderableMesh<TPerVert, TPerPrim>.IDynamic> 
        _renderableMeshes = new(Enum.GetValues<PrimitiveTopology>().Length);

    private DynamicRenderableModel(IGraphicApi graphicApi) => _graphicApi = graphicApi;
    
    public static ResultOrError<DynamicRenderableModel<TPerVert, TPerPrim>> Create(IGraphicApi graphicApi)
    {
        var model = new DynamicRenderableModel<TPerVert, TPerPrim>(graphicApi);
        // filling both
        foreach (var topology in Enum.GetValues<PrimitiveTopology>())
        {
            var renderableOrErr = graphicApi.CreateDynamicRenderable<TPerVert, TPerPrim>();
            if (renderableOrErr.Error is not null)
                return renderableOrErr.Error.Value;
         
            model._meshes[topology] = new Mesh<TPerVert, TPerPrim>(topology);
            model._renderableMeshes[topology] = renderableOrErr.GetOrThrow();
        }
        return ResultOrError.Success(model);
    }
    
    public void Add(PrimitiveTopology topology, Action<Mesh<TPerVert, TPerPrim>> meshModifier) 
        => meshModifier(_meshes[topology]);

    public void Clear()
    {
        foreach (var (_, mesh) in _meshes) 
            mesh.Clear();
    }
    
    /// <summary>
    /// Updates renderable based on recent changes in builders
    /// </summary>
    public void Refit()
    {
        foreach (var (primitive, mesh) in _meshes) 
            _renderableMeshes[primitive].Refit(mesh);
    }

    public void Render(IShader<TPerVert, TPerPrim> shader)
    {
        foreach (var (_, renderableMesh) in _renderableMeshes) 
            shader.Render(renderableMesh);
    }
}