using OpenTK.Mathematics;

namespace Enjune.Graphic.Api;

public interface ITexture
{
    public Vector2i Size { get; }
}

public interface ITextureArray : ITexture
{
    public int Layers { get; }
}

public interface IDynamicTexture : ITexture
{
    public void BindAsRenderTarget();
    public new Vector2i Size { get; set; }
}