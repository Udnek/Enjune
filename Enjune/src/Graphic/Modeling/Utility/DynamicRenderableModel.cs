using Enjune.Graphic.Api;
using Enjune.Graphic.Modeling.Utility;
using Enjune.Misc;

namespace Enjune.Graphic.Modeling;

/// <summary>
/// Basically:
/// - Create
/// - while you need:
///     - Clear builders
///     - Add meshes
///     - Call refit
///     - Render
/// </summary>
public class DynamicRenderableModel : IRenderableModel
{
    private readonly IGraphicApi _graphicApi;
    private Dictionary<PrimitiveTopology, MeshInstance.Builder> _meshes = new();
    private Dictionary<PrimitiveTopology, IRenderableMesh.IDynamic> _renderableMeshes = new();

    public DynamicRenderableModel(IGraphicApi graphicApi)
    {
        _graphicApi = graphicApi;
        // filling both
        foreach (var topology in Enum.GetValues<PrimitiveTopology>())
        {
            _meshes[topology] = new MeshInstance.Builder(topology);
            _renderableMeshes[topology] = graphicApi.CreateDynamicRenderable(MeshInstance.CreateEmpty(topology));
        }
    }

    public void Add(MeshInstance.Entry entry)
    {
        _meshes[entry.Geometry.Topology].Add(entry);
    }
    
    public void Clear()
    {
        foreach (var (_, builder) in _meshes)
        {
            builder.Clear();
        }
        // not clearing renderables: we will reuse them
    }
    
    /// <summary>
    /// Updates renderable based on recent changes in builders
    /// </summary>
    public void Refit()
    {
        foreach (var (primitive, builder) in _meshes)
        {
            _renderableMeshes[primitive].Refit(builder.Build(false));
        }
    }

    public void Render(IShader shader)
    {
        foreach (var (_, renderableMesh) in _renderableMeshes) 
            shader.Render(renderableMesh);
    }
}