namespace Enjune.Graphic.GraphicApi;

public interface IRenderableModel : IDisposable
{
    public interface IMaterial : IRenderableModel
    {
        public void Render(IShader.ICamera.IMaterial shader);
        public void Render(IShader.IShadowMap shader);
    }
    
    public interface IColor : IRenderableModel
    {
        public void Render(IShader.ICamera.IColor shader, IGraphicApi.Primitive primitive = IGraphicApi.Primitive.Line);
    }
}