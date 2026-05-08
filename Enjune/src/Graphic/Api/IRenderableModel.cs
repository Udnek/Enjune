namespace Enjune.Graphic.Api;

public interface IRenderableModel : IDisposable
{
    public IGraphicApi.Primitive CurrentPrimitive { get; }

    public void Render(IShader.ICamera.IColor shader);
    public void Render(IShader.ICamera.IMaterial shader);
    public void Render(IShader.IShadowMap shader);
    
    public interface IDynamic : IRenderableModel
    {
        public void Refit(Model model, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Triangle);
    }
}