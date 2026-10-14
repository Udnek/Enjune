using Enjune.Graphic.Modeling;

namespace Enjune.Graphic.Api;

public interface IShader<TPerVert, TPerPrim, TVaryings> where TPerVert : unmanaged where TPerPrim : unmanaged
{
    public void Render(IRenderableMesh<TPerVert, TPerPrim> mesh);
}