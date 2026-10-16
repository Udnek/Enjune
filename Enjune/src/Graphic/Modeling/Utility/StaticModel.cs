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
            var meshOrError = graphicApi.CreateRenderable(mesh);
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
         /// <param name="meshModifier"></param>
         /// <returns></returns>
         public Builder Add(PrimitiveTopology topology, Action<Mesh<TPerVert, TPerPrim>> meshModifier)
         {
             if (_meshes.TryGetValue(topology, out var mesh))
             {
                 meshModifier(mesh);
                 return this;
             }

             var newMesh = new Mesh<TPerVert, TPerPrim>(topology);
             _meshes[topology] = newMesh;
             meshModifier(newMesh);
             return this;
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