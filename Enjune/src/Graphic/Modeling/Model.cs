using System.Diagnostics.Contracts;
using Enjune.Graphic.Asset;
using Enjune.Misc;

namespace Enjune.Graphic.Modeling;

public sealed class Model
{
    public readonly Entry[] Meshes;

    public Model(Entry[] meshes)
    {
        if (meshes.Length == 0)
            Logger.Warn(this, "constructing empty model");
        Meshes = meshes;
    }

    public Model(Mesh mesh, PerMesh perMesh) : this([new Entry(mesh, perMesh)]){}

    [Pure]
    public string Info()
    {
        return $"meshes: {Meshes.Length}; " +
               $"vertices: {Meshes.Select(mc => mc.Mesh.Vertices.Length).Sum()}; " +
               $"triangles: {Meshes.Select(mc => mc.Mesh.Indexes.Length).Sum()/3};";
    }

    public record struct Entry(Mesh Mesh, PerMesh PerMesh);

    public record struct PerMesh(CompiledMaterial? Material, Color MeshColor)
    {
        public PerMesh(CompiledMaterial material) : this(material, Color.One){}
        public PerMesh(Color color) : this(null, color){}
        public PerMesh() : this(null, Color.One){}
    }
    
    public class Builder
    {
        private readonly List<Entry> _meshes = [];
        public bool IsEmpty => _meshes.Count == 0;
        
        public Builder Add(Mesh mesh, PerMesh perMeshData)
        {
            _meshes.Add(new Entry(mesh, perMeshData));
            return this;
        }
        
        [Pure]
        public Model Build(bool mergeSimilarMeshes = true)
        {
            if (!mergeSimilarMeshes)
                return new Model(_meshes.ToArray());
            
            List<Entry> newMeshes = [];
            foreach (var groupedByMat in _meshes.GroupBy(e => e.PerMesh))
            {
                var mat = groupedByMat.Key;
                var mergedMesh = Mesh.Merge(groupedByMat.Select(e => e.Mesh));
                newMeshes.Add(new Entry(mergedMesh, mat));
            }

            return new Model(newMeshes.ToArray());
        }
    }
}