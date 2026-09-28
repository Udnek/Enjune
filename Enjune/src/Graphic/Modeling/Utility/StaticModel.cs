using System.Diagnostics.Contracts;
using Enjune.Graphic.Api;

namespace Enjune.Graphic.Modeling;

public class StaticModel
{
    public ReadOnlySpan<MeshInstance> Meshes => _meshes.AsSpan();
    private MeshInstance[] _meshes;

    public StaticModel(params MeshInstance[] meshes) => _meshes = meshes;

    [Pure]
    public StaticRenderableModel CreateRenderable(IGraphicApi graphicApi)
    {
        var renderables = Array.ConvertAll(_meshes, graphicApi.CreateStaticRenderable);
        return new StaticRenderableModel(renderables);
    }

    public sealed class Builder
    {
         private readonly Dictionary<PrimitiveTopology, MeshInstance.Builder> _meshes = new();
         
         public Builder Add(MeshInstance.Entry mesh)
         {
             var topology = mesh.Geometry.Topology;
             if (_meshes.TryGetValue(topology, out var builder))
             {
                 builder.Add(mesh);
                 return this;
             }

             _meshes[topology] = new MeshInstance.Builder(topology).Add(mesh);
             return this;
         }

         [Pure]
         public StaticModel Build(bool mergeSimilar) 
             => new(_meshes.Select(b => b.Value.Build(mergeSimilar)).ToArray());
    }
}