using System.Diagnostics.Contracts;
using Enjune.Graphic.Asset;
using Enjune.Misc;

namespace Enjune.Graphic;

public class Model<TPerVert, TPerMesh>
{
    public (Mesh<TPerVert> mesh, TPerMesh perMesh)[] Meshes = [];
    
    public Model(ValueTuple<Mesh<TPerVert>, TPerMesh>[] meshes)
    {
        if (meshes.Length == 0)
            Logger.Warn(this,"constructing empty model");
        
    }
    

    [Pure]
    public string Info()
    {
        return $"meshes: {Meshes.Length}; " +
               $"vertices: {Meshes.Select(mc => mc.mesh.Vertices.Length).Sum()}; " +
               $"triangles: {Meshes.Select(mc => mc.mesh.Indexes.Length).Sum()/3};";
    }
}

public static class Model
{
    public class Builder<TModel, TPerVert, TPerMesh> where TModel : Model<TPerVert, TPerMesh>, new()
    {
        private readonly List<(Mesh<TPerVert> mesh, TPerMesh perMesh)> _meshes = [];
        
        public bool IsEmpty => _meshes.Count == 0;
        
        public Builder<TModel, TPerVert, TPerMesh> Add(Mesh<TPerVert> mesh, TPerMesh perMeshData)
        {
            _meshes.Add((mesh, perMeshData));
            return this;
        }

        [Pure]
        public Model<TPerVert, TPerMesh> Build(bool mergeSimilarMeshes = true)
        {
            if (mergeSimilarMeshes)
            {
                return new TModel
                {
                    Meshes = _meshes.ToArray()
                };
            }
            List<(Mesh<TPerVert> Mesh, TPerMesh PerMesh)> newMeshes = [];
            foreach (var groupedByMat in _meshes.GroupBy(e => e.perMesh))
            {
                var mat = groupedByMat.Key;
                var mergedMesh = Mesh.Merge(groupedByMat.Select(e => e.mesh));
                newMeshes.Add((mergedMesh, mat));
            }

            return new TModel
            {
                Meshes = newMeshes.ToArray()
            };
        }
    }
}