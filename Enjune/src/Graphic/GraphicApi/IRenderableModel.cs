namespace Enjune.Graphic.GraphicApi;

public interface IRenderableModel : IDisposable
{
    public void Render(IShader.ICamera.IColor shader);
    public void Render(IShader.ICamera.IMaterial shader);
    public void Render(IShader.IShadowMap shader);
    
    public interface IDynamic : IRenderableModel
    {
        public void Refit(MaterialModel model, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle);
        public void Refit(ColorModel model, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle);
    }
}