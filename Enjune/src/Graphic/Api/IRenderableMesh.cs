using Enjune.Graphic.Modeling;

namespace Enjune.Graphic.Api;

/// <summary>
/// Should be rendered in shader
/// </summary>
public interface IRenderableMesh
{
    /// <summary>
    /// Can be refit to reuse memory
    /// </summary>
    public interface IDynamic : IRenderableMesh
    {
        public void Refit(MeshInstance mesh);
    }
}