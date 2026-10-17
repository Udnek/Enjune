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

    public DynamicRenderableModel(IGraphicApi graphicApi) => _graphicApi = graphicApi;

    /// <summary>
    /// Can be used to modify mesh by topology
    /// </summary>
    /// <param name="topology"></param>
    public Mesh<TPerVert, TPerPrim> GetMesh(PrimitiveTopology topology)
    {
        if (_meshes.TryGetValue(topology, out var alreadyMesh))
            return alreadyMesh;

        var newMesh = new Mesh<TPerVert,TPerPrim>(topology);
        var renderableOrErr = _graphicApi.CreateDynamicRenderableMesh<TPerVert, TPerPrim>();
        if (renderableOrErr.Error is not null)
        {
            Logger.Error(this, renderableOrErr.Error);
            return null!;
        }

        _renderableMeshes[topology] = renderableOrErr.GetOrThrow();
        _meshes[topology] = newMesh;
        return newMesh;
    }

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