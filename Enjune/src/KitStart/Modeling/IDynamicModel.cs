using Enjune.Graphic.Modeling;
using Enjune.Misc;

namespace Enjune.KitStart.Modeling;

public class DynamicModel<TPerVert, TPerPrim> where TPerVert : unmanaged where TPerPrim : unmanaged
{
    public readonly Dictionary<PrimitiveTopology, Mesh<TPerVert, TPerPrim>> 
        Meshes = new(Enum.GetValues<PrimitiveTopology>().Length);
    
    /// <summary>
    /// Returns or creates and returns appropriate mesh
    /// </summary>
    public Mesh<TPerVert, TPerPrim> GetMesh(PrimitiveTopology topology)
    {
        if (Meshes.TryGetValue(topology, out var alreadyMesh))
            return alreadyMesh;
        var mesh = new Mesh<TPerVert,TPerPrim>(topology);
        Meshes[topology] = mesh;
        return mesh;
    }

    /// <summary>
    /// Can be used to modify mesh by topology
    /// </summary>
    public void Clear()
    {
        foreach (var (_, mesh) in Meshes) 
            mesh.Clear();
    }
}