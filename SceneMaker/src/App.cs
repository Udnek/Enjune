using Enjune;
using Enjune.File;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Key;
using Enjune.KitStart;
using Enjune.Misc;
using Enjune.World;
using OpenGLApi;
using OpenTK.Mathematics;
using SceneMaker.Misc;
using SceneMaker.Ui;

namespace SceneMaker;

public class App : AbstractDisposable, IApp
{
    private static readonly Vector2i InitialWindowSize = (480*2, 360*2);
    
    [DisposeAtLast("other objects may cause segfault when disposing //TODO fix")] // TODO probably dispose all GlModels in GlApi itself?
    public IGraphicApi Grapi = null!;
    
    private readonly KeyBinds _binds;
    private readonly Wasd _wasd;
    private readonly KeyBinds.Bind _dumbTexturesBind;
    private readonly KeyBinds.Bind _freeCursorBind;
    private readonly KeyBinds.Bind _lockCursorBind;
    
    public readonly BasicInputHandler InputHandler;
    public FlyingPlayerController WasdController { get; private set; } = null!;
    public EditorController EditorController { get; private set; } = null!;
    
    //private Scene _scene = null!;
    private UiManager _uiManager = null!;
    //public Focus Focused = Focus.Scene;

    public App()
    {
        _binds = KeyBinds.CreateEmpty();
        _wasd = Wasd.AddTo(_binds);
        _freeCursorBind = _binds.AddBind(new KeyBinds.Bind("free_cursor", KeyCode.Escape));
        _lockCursorBind = _binds.AddBind(new KeyBinds.Bind("lock_cursor", KeyCode.RightMouseButton));
        
        InputHandler = new BasicInputHandler(InitialWindowSize, 0.5f);
        _dumbTexturesBind = _binds.AddBind(new KeyBinds.Bind("dumb_textures", KeyCode.F2));
    }

    public Error? Init()
    {
        var assetManager = new AssetManager();

        // font
        var font = assetManager.AddFont(AssemblyPath.Of(Enjune.Enjune.Assembly, "Fonts", "vt323.ttf"), 128, out var error);
        if (font == null) return error;

        // scene load
        {
            var result = SceneManager.Load(assetManager);
            if (result.Scene is null) return result.Error;
            _scene = result.Scene;
        }
        
        var assets = assetManager.Compile();

        // grapi
        {
            var grapi = new OpenGlApi().Init(assets, InitialWindowSize, "Scene Maker", InputHandler, out error);
            if (grapi == null) return error;
            Grapi = grapi;
        }
        Grapi.SetVsync(false);
        Grapi.SetClearColor(new Color(0.2f, 0.2f, 0.2f, 0f));
        Grapi.SetCursorMode(IGraphicApi.CursorMode.Centered);
        
        // adding models
        foreach (var obj in _scene.Objects)
        {
            var model = obj.Model?.GetOrNull();
            if (model is null) continue;
            obj.RenderableModel = Grapi.CreateStaticRenderable(model);
        }

        // controllers
        {
            WasdController = new FlyingPlayerController(Grapi, InputHandler, _wasd, 0.2f);
            EditorController = new EditorController(Grapi, InputHandler, _scene);
            _scene.Objects.Add(EditorController.AxisObject);
        }

        _uiManager = new UiManager(this, font);
        
        return null;
    }
    
    public enum Focus
    {
        Scene,
        ObjectInspector
    }

    public void MainCycle()
    {
        GraphicCycle();
        SceneManager.Save(_scene);
    }

    private void GraphicCycle()
    {
        var projection = Matrix4.CreatePerspectiveFieldOfView(
            MathF.PI / 2, (float) InputHandler.WindowSize.X / InputHandler.WindowSize.Y, 0.1f, 100f);
        var view = WasdController.View;
        
        // cache to not create new list every frame
        List<SpotLight> spotLights = [];
        
        Utils.RunTargetFpsLoopWhile(500,
            () => !Grapi.ShouldStop(),
            deltaTime =>
            {
                InputHandler.PrepareAtFrameStart();
                
                // window size change
                if (InputHandler.WindowSizeChanged)
                {
                    Grapi.SetRenderSize(InputHandler.WindowSize);
                    projection = Matrix4.CreatePerspectiveFieldOfView(
                        MathF.PI / 2, (float) InputHandler.WindowSize.X / InputHandler.WindowSize.Y, 0.1f, 100f);
                }
                
                // UI
                _uiManager.Update(deltaTime); 
                
                // wasd && editor controller
                if (!_uiManager.Ui.IsFocused)
                {
                    WasdController.Update(deltaTime);
                    view = WasdController.View;
                    
                    EditorController.Update(view, projection);
                }
                
                // genera; keyboard input
                if (InputHandler.IsPressed(_freeCursorBind))
                    Grapi.SetCursorMode(IGraphicApi.CursorMode.Normal);
                else if (InputHandler.IsPressed(_lockCursorBind))
                    Grapi.SetCursorMode(IGraphicApi.CursorMode.Centered);
                
                if (InputHandler.IsPressed(_dumbTexturesBind)) 
                    Grapi.DumpTextures(ExternalPath.Of("."));
                
                // render

                #region Lights
                {
                    spotLights.Clear();
                    foreach (var sObject in _scene.Objects)
                    {
                        var light = sObject.SpotLight;
                        if (light is null) continue;
                        light.Position = sObject.Position;
                        light.UpdateView();
                        spotLights.Add(light);
                    }
                    Grapi.SetLights(spotLights.AsSpan());
                }
                #endregion

                #region Shadows
                Grapi.UseShader<IShader.IShadowMap>(s =>
                {
                    s.ForEachLight(() =>
                    {
                        Grapi.ClearRenderBuffer();
                        foreach (var obj in _scene.Objects)
                        {
                            if (!obj.IsRealistic) continue;
                            if (obj.Hidden) continue;
                            if (obj.RenderableModel is null) continue;
                            if (obj.SpotLight is not null) continue;
                            
                            s.ModelTransform(obj.ModelTransform);
                            obj.RenderableModel.Render(s);
                        }
                    });
                });
                #endregion
                
                #region Material
                Grapi.UseShader<IShader.ICamera.IMaterial>(s =>
                {
                    Grapi.ClearRenderBuffer();
                    s.ProjectionTransform(projection);
                    s.ViewTransform(view);
                    s.ViewPosition(WasdController.Position);
                    
                    foreach (var obj in _scene.Objects)
                    {
                        if (!obj.IsRealistic) continue;
                        if (obj.Hidden) continue;
                        if (obj.RenderableModel is null) continue;
                    
                        if (obj == EditorController.SelectedObject)
                            s.GlobalColor((1, 0.5f, 0f, 1f));
                        else
                            s.GlobalColor(new Color(1f));
                    
                        s.ModelTransform(obj.ModelTransform);
                        obj.RenderableModel.Render(s);
                    }
                });
                #endregion
                
                #region Flat Color
                Grapi.UseShader<IShader.ICamera.IColor>(s =>
                {
                    // drawing everything else over
                    Grapi.ClearRenderBuffer(false);
                    s.ProjectionTransform(projection);
                    s.ViewTransform(view);
                    
                    foreach (var obj in _scene.Objects)
                    {
                        if (obj.IsRealistic) continue;
                        if (obj.Hidden) continue;
                        if (obj.RenderableModel is null) continue;
                
                        s.ModelTransform(obj.ModelTransform);
                
                        obj.RenderableModel.Render(s);
                    }
                    
                    // render UI
                    Grapi.ClearRenderBuffer(false, true);
                    _uiManager.Ui.Render(s);
                });
                #endregion
                
                // end
                Grapi.UpdateScreen();
                InputHandler.ClearForNextFrame();
                Grapi.UpdateEvents();
            });
    }

    protected override void DisposeData()
    {
        foreach (var o in _scene.Objects) 
            o.RenderableModel?.Dispose();
        
        Utils.DisposeAllFields(this);
    }
}