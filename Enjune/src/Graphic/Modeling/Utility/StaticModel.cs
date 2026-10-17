using System.Diagnostics.Contracts;
using Enjune.Graphic.Api;
using Enjune.Misc;

namespace Enjune.Graphic.Modeling.Utility;

public class StaticModel<TPerVert, TPerPrim> where TPerVert : unmanaged where TPerPrim : unmanaged
{
    public ReadOnlySpan<Mesh<TPerVert, TPerPrim>> Meshes => _meshes.AsSpan();
    private Mesh<TPerVert, TPerPrim>[] _meshes;

    public StaticModel(params Mesh<TPerVert, TPerPrim>[] meshes) => _meshes = meshes;

    public ResultOrError<StaticRenderableModel<TPerVert, TPerPrim>> CreateRenderable(IGraphicApi graphicApi)
    {
        var renderables = new IRenderableMesh<TPerVert, TPerPrim>[_meshes.Length];
        foreach (var mesh in _meshes)
        {
            var meshOrError = graphicApi.CreateRenderableMesh(mesh);
            if (meshOrError.Error is not null)
                return meshOrError.Error.Value;
        }
        
        return ResultOrError.Success(new StaticRenderableModel<TPerVert, TPerPrim>(renderables));
    }

    public sealed class Builder
    {
         private readonly Dictionary<PrimitiveTopology, Mesh<TPerVert, TPerPrim>> _meshes = new();

         /// <summary>
         /// Returns mesh with appropriate topology to be modified
         /// </summary>
         /// <param name="topology"></param>
         /// <returns></returns>
         public Mesh<TPerVert, TPerPrim> GetMeshWithTopology(PrimitiveTopology topology)
         {
             if (_meshes.TryGetValue(topology, out var mesh))
                 return mesh;

             var newMesh = new Mesh<TPerVert, TPerPrim>(topology);
             _meshes[topology] = newMesh;
             return newMesh;
         }

         [Pure]
         public StaticModel<TPerVert, TPerPrim> Build()
         {
             return new StaticModel<TPerVert, TPerPrim>(_meshes.Values
                 .Where(m => !m.IsEmpty())
                 .Select(m => 
                 {
                     // optimizing memory usage
                     m.TrimExcess();
                     return m;
                 }).ToArray());
         }
    }
}