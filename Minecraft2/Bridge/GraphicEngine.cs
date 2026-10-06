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
    private readonly KeyBinds.Bind _freeCursorBind;
    private readonly KeyBinds.Bind _lockCursorBind;
    
    public GraphicEngine(App app)
    {
        _app = app;
        Projection = Matrix4.CreatePerspectiveFieldOfView(
            MathF.PI / 2, (float) _app.InputHandler.WindowSize.X / _app.InputHandler.WindowSize.Y, 0.1f, 100f);
        _freeCursorBind = _app.Binds.AddBind(new KeyBinds.Bind("free_cursor", KeyCode.Escape));
        _lockCursorBind = _app.Binds.AddBind(new KeyBinds.Bind("lock_cursor", KeyCode.RightMouseButton));
    }

    public void Update()
    {
        var inputHandler = _app.InputHandler;
        var graphicApi = _app.GraphicApi;
        View = _app.WasdController.View;
        
        // window size change
        if (inputHandler.WindowSizeChanged)
        {
            _app.GraphicApi.SetRenderSize(inputHandler.WindowSize);
            Projection = Matrix4.CreatePerspectiveFieldOfView(
                MathF.PI / 2, (float) inputHandler.WindowSize.X / inputHandler.WindowSize.Y, 0.1f, 100f);
        }
        
        // input
        if (inputHandler.IsPressed(_freeCursorBind))
            graphicApi.SetCursorMode(IGraphicApi.CursorMode.Normal);
        else if (inputHandler.IsPressed(_lockCursorBind))
            graphicApi.SetCursorMode(IGraphicApi.CursorMode.Centered);
        
        // render

        #region Lights
        {
            graphicApi.SetLights(SpotLights.Values);
        }
        #endregion

        #region Shadows
        graphicApi.UseShader<IShader.IShadowMap>(s =>
        {
            s.ForEachLight(() =>
            {
                graphicApi.ClearRenderBuffer();
                foreach (var obj in Objects.Values)
                {
                    s.ModelTransform(obj.TransformMatrix);
                    obj.Model.Render(s);
                }
            });
        });
        #endregion
        
        #region Material
        graphicApi.UseShader<IShader.ICamera.IMaterial>(s =>
        {
            graphicApi.ClearRenderBuffer();
            s.ProjectionTransform(Projection);
            s.ViewTransform(View);
            s.ViewPosition(_app.WasdController.Position);
            
            foreach (var obj in Objects.Values)
            {
                s.ModelTransform(obj.TransformMatrix);
                obj.Model.Render(s);
            }
        });
        #endregion
        
        #region Flat Color
        graphicApi.UseShader<IShader.ICamera.IColor>(s =>
        {
            // drawing everything else over
            graphicApi.ClearRenderBuffer(false);
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