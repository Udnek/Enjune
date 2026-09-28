using Enjune.Graphic.Api;
using Enjune.Graphic.Modeling.Utility;
using Enjune.Misc;

namespace Enjune.Graphic.Modeling;

public class StaticRenderableModel : AbstractDisposable, IRenderableModel
{
    private readonly IRenderableMesh[] _meshes;

    public StaticRenderableModel(params IRenderableMesh[] meshes)
    {
        _meshes = meshes;
    }
    
    public void Render(IShader shader)
    {
        foreach (var mesh in _meshes)
        {
            shader.Render(mesh);
        }
    }

    protected override void DisposeData()
    {
        _meshes.ForEach(m => m.Dispose());
    }
}