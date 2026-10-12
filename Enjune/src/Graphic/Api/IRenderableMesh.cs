using Enjune.Graphic.Modeling;

namespace Enjune.Graphic.Api;

/// <summary>
/// Should be rendered in shader
/// </summary>
public interface IRenderableMesh<TPerVert, TPerPrim> where TPerVert : unmanaged where TPerPrim : unmanaged
{
    /// <summary>
    /// Can be refit to reuse memory
    /// </summary>
    public interface IDynamic : IRenderableMesh<TPerVert, TPerPrim>
    {
        public void Refit(MeshInstance mesh);
    }
}