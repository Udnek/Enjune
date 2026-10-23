using Enjune.Graphic.Api;
using Enjune.Graphic.Modeling;
using Enjune.Misc;

namespace Enjune.KitStart.Modeling;

public class DynamicRenderableModel<TPerVert, TPerPrim> : IRenderableModel<TPerVert, TPerPrim> where TPerVert : unmanaged where TPerPrim : unmanaged
{
    private readonly Dictionary<PrimitiveTopology, IRenderableMesh<TPerVert, TPerPrim>.IDynamic> 
        _renderableMeshes = new(Enum.GetValues<PrimitiveTopology>().Length);

    private DynamicRenderableModel(){}

    public static ResultOrError<DynamicRenderableModel<TPerVert, TPerPrim>> Create(IGraphicApi graphicApi)
    {
        var model = new DynamicRenderableModel<TPerVert, TPerPrim>();
        foreach (var topology in Enum.GetValues<PrimitiveTopology>())
        {
            if (graphicApi.CreateDynamicRenderableMesh<TPerVert, TPerPrim>().GetIsFailed(out var mesh, out var error))
                return new Error("can not create mesh: " + error);
            model._renderableMeshes[topology] = mesh;
        }

        return ResultOrError.Success(model);
    }
    
    
    /// <summary>
    /// Updates renderable based on recent changes in builders
    /// </summary>
    public void Refit(DynamicModel<TPerVert, TPerPrim>? model)
    {
        if (model is null)
        {
            foreach (var (_, mesh) in _renderableMeshes) 
                mesh.Refit(null);
        }
        else
        {
            foreach (var (topology, mesh) in model.Meshes) 
                _renderableMeshes[topology].Refit(mesh);
        }
    }

    public void Render(IShader<TPerVert, TPerPrim> shader)
    {
        foreach (var (_, renderableMesh) in _renderableMeshes) 
            shader.Render(renderableMesh);
    }
}