using Enjune.Graphic.Api;

namespace Enjune.Graphic.Modeling.Utility;

public class StaticRenderableModel<TPerVert, TPerPRim> : IRenderableModel<TPerVert, TPerPRim> where TPerVert : unmanaged where TPerPRim : unmanaged
{
    private readonly IRenderableMesh<TPerVert, TPerPRim>[] _meshes;

    public StaticRenderableModel(params IRenderableMesh<TPerVert, TPerPRim>[] meshes)
    {
        _meshes = meshes;
    }

    public void Render(IShader<TPerVert, TPerPRim> shader)
    {
        foreach (var mesh in _meshes) 
            shader.Render(mesh);
    }
}