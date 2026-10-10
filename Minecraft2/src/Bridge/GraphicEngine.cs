using Enjune.Graphic.Api;
using Enjune.Graphic.Key;

namespace Minecraft2.Bridge;

public class GraphicEngine
{

    #region Public

    public readonly Dictionary<Guid, GraphicObject> Objects = [];
    public readonly Dictionary<Guid, SpotLight> SpotLights = [];
    public Matrix4 Projection { get; private set; }
    public Matrix4 View { get; private set; }

    #endregion

    private readonly App _app;
    
    public GraphicEngine(App app)
    {
        _app = app;
        Projection = Matrix4.CreatePerspectiveFieldOfView(
            MathF.PI / 2, (float) _app.InputHandler.WindowSize.X / _app.InputHandler.WindowSize.Y, 0.1f, 1000f);
    }

    public void Update()
    {
        var inputHandler = _app.InputHandler;
        var graphicApi = _app.GraphicApi;
        View = _app.FlyingController.View;
        
        // window size change
        if (inputHandler.WindowSizeChanged)
        {
            _app.GraphicApi.SetRenderSize(inputHandler.WindowSize);
            Projection = Matrix4.CreatePerspectiveFieldOfView(
                MathF.PI / 2, (float) inputHandler.WindowSize.X / inputHandler.WindowSize.Y, 0.1f, 1000f);
        }
        
        // render
        
        #region Flat Color
        
        graphicApi.UseShader<IShader.ICamera.IColor>(s =>
        {
            graphicApi.ClearRenderBuffer();
            // drawing everything else over
            s.ProjectionTransform(Projection);
            s.ViewTransform(View);
            
            foreach (var obj in Objects.Values)
            {
                s.ModelTransform(obj.TransformMatrix);
                obj.Model.Render(s);
            }
        });
        #endregion
    }
}