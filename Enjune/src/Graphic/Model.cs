using System.Diagnostics.Contracts;
using Enjune.Graphic.Asset;
using Enjune.Misc;

namespace Enjune.Graphic;

public sealed class Model<TPerVert, TPerMesh>
{
    public readonly (Mesh<TPerVert> Mesh, TPerMesh PerMesh)[] Meshes;

    protected Model(ValueTuple<Mesh<TPerVert>, TPerMesh>[] meshes)
    {
        if (meshes.Length == 0)
            Logger.Warn(this,"constructing empty model");
        Meshes = meshes;
    }

    public static Model<TPerVert, TPerMesh> CreateFromOneMaterial(Mesh<TPerVert>[] meshes, TPerMesh perMeshData)
        => CreateNotOptimized([(Mesh.Merge(meshes), perMeshData)]);
    
    public static Model<TPerVert, TPerMesh> CreateAndOptimize(ValueTuple<Mesh<TPerVert>, TPerMesh>[] meshes)
    {
        List<ValueTuple<Mesh<TPerVert>, TPerMesh>> newMeshes = [];
        foreach (var groupedByMat in meshes.GroupBy(e => e.Item2))
        {
            var mat = groupedByMat.Key;
            var mergedMesh = Mesh.Merge(groupedByMat.Select(e => e.Item1));
            newMeshes.Add((mergedMesh, mat));
        }
        return CreateNotOptimized(newMeshes.ToArray());
    }
    
    public static Model<TPerVert, TPerMesh> CreateNotOptimized(ValueTuple<Mesh<TPerVert>, TPerMesh>[] meshes) 
        => new(meshes);


    public string Info()
    {
        return $"meshes: {Meshes.Length}; " +
               $"vertices: {Meshes.Select(mc => mc.Mesh.Vertices.Length).Sum()}; " +
               $"triangles: {Meshes.Select(mc => mc.Mesh.Indexes.Length).Sum()/3};";
    }

    public class Builder
    {
        private readonly List<ValueTuple<Mesh<TPerVert>, TPerMesh>> _meshes = [];
        
        public bool IsEmpty => _meshes.Count == 0;
        
        public Builder Add(Mesh<TPerVert> mesh, TPerMesh perMeshData)
        {
            _meshes.Add((mesh, perMeshData));
            return this;
        }

        [Pure]
        public Model<TPerVert, TPerMesh> Build(bool mergeSimilarMeshes = true) 
            => mergeSimilarMeshes ? CreateAndOptimize(_meshes.ToArray()) : CreateNotOptimized(_meshes.ToArray());
    }
}