namespace Enjune.Graphic.Api;

public interface ITextureSlot<in TexType> where TexType : ITexture
{
    public void BindToSample(TexType texture);
}