using Enjune.Graphic.Asset;
using Enjune.Misc;

namespace Enjune.Graphic;

public sealed class Model
{
    public ValueTuple<Mesh, CompiledMaterial>[] Meshes;

    private Model(ValueTuple<Mesh, CompiledMaterial>[] meshes)
    {
        if (meshes.Length == 0)
            Logger.Error(this,"constructing empty model");
        Meshes = meshes;
    }

    public static Model CreateFromOneMaterial(Mesh[] meshes, CompiledMaterial material)
        => CreateNotOptimized([(Mesh.Merge(meshes), material)]);
    
    public static Model CreateAndOptimize(ValueTuple<Mesh, CompiledMaterial>[] meshes)
    {
        List<ValueTuple<Mesh, CompiledMaterial>> newMeshes = [];
        foreach (var groupedByMat in meshes.GroupBy(e => e.Item2))
        {
            var mat = groupedByMat.Key;
            var mergedMesh = Mesh.Merge(groupedByMat.Select(e => e.Item1));
            newMeshes.Add((mergedMesh, mat));
        }
        return CreateNotOptimized(newMeshes.ToArray());
    }
    
    public static Model CreateNotOptimized(ValueTuple<Mesh, CompiledMaterial>[] meshes) 
        => new(meshes);


    public string Info()
    {
        return $"meshes: {Meshes.Length}; " +
               $"vertices: {Meshes.Select(mc => mc.Item1.Vertices.Length).Sum()}; " +
               $"triangles: {Meshes.Select(mc => mc.Item1.Indexes.Length).Sum()/3};";
    }

    public class Builder
    {
        private readonly List<ValueTuple<Mesh, CompiledMaterial>> _meshes = [];
        
        public bool IsEmpty => _meshes.Count == 0;
        
        public Builder Add(Mesh mesh, CompiledMaterial material)
        {
            _meshes.Add((mesh, material));
            return this;
        }

        public Model Build() => CreateAndOptimize(_meshes.ToArray());
    }
}