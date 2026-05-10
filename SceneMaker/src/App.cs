using System.Diagnostics;
using System.Globalization;
using Enjune;
using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Font;
using Enjune.Graphic.Key;
using Enjune.Graphic.UI;
using Enjune.KitStart;
using Enjune.Misc;
using Enjune.World;
using OpenGLApi;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SceneMaker;

public class App : AbstractDisposable, IApp
{
    private static readonly Vector2i InitialWindowSize = (480*2, 360*2);
    
    private IGraphicApi _grapi;
    private readonly KeyBinds _binds;
    private readonly Wasd _wasd;
    private readonly KeyBinds.Bind _dumbTexturesBind;
    
    private readonly BasicInputHandler _inputHandler;
    private FlyingPlayerController _wasdController;
    
    private readonly KeyBinds.Bind _freeCursorBind;
    private readonly KeyBinds.Bind _lockCursorBind;
    private readonly Scene _scene;
    private EditorController _editorController = null!;
    private IRenderableModel.IDynamic _uiModel;
    private Ui _ui;
    private UiText _fpsUiElement;

    public App()
    {
        _binds = KeyBinds.CreateEmpty();
        _wasd = Wasd.AddTo(_binds);
        _freeCursorBind = _binds.AddBind(new KeyBinds.Bind("free_cursor", KeyCode.Escape));
        _lockCursorBind = _binds.AddBind(new KeyBinds.Bind("lock_cursor", KeyCode.RightMouseButton));
        
        _dumbTexturesBind = _binds.AddBind(new KeyBinds.Bind("dumb_textures", KeyCode.F2));
        _inputHandler = new BasicInputHandler(_binds, InitialWindowSize, 0.5f);
        _scene = new Scene();
    }

    public Error? Init()
    {
        var assetManager = new AssetManager();

        // var watchTower
        //     = new DotObjModelReader(assetManager,
        //             AssemblyPath.Of(Enjune.Enjune.Assembly, "Models", "wt", "wooden watch tower2.obj"))
        //         .Read(out var error);
        // if (watchTower == null) return error;
        // watchTower.Meshes[0].PerMesh.Raw.Color = (1, 1, 1, 1);
        
        
        // _scene.Objects.Add(new SObject(watchTower)
        // {
        //     Rotation = Quaternion.FromEulerAngles(0, MathHelper.DegreesToRadians(45), MathHelper.DegreesToRadians(45))
        // });

        // var mapModel 
        //     = new DotMapReader(assetManager, AssemblyPath.Of(Enjune.Enjune.Assembly, "Maps", "test.map"))
        //         .Read(out error);
        // if (mapModel == null) return error;
        // _scene.Objects.Add(new SObject()
        // {
        //     MatModel = mapModel,
        //     Scale = Vector3.One * 1/16f,
        //     Rotation = Quaternion.FromEulerAngles(new Vector3(MathHelper.DegreesToRadians(-90), 0, 0))
        // });
        
        
        var calavera = new DotGlbReader(assetManager, AssemblyPath.Of(Enjune.Enjune.Assembly, "Models", "Calavera", "Calavera.glb"))
            .Read(out var error);
        if (calavera == null) return error;
        

        // var toyCar = new DotGlbReader(assetManager, AssemblyPath.Of(Enjune.Enjune.Assembly, "Models", "ToyCar.glb"))
        //     .Read(out error);
        // if (toyCar == null) return error;
        // _scene.Objects.Add(new SObject(toyCar)
        // {
        //     Scale = new Vector3(0.05f),
        //     Rotation = Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(90), 0, 0)
        // });
        //
        // Logger.Log(this, $"{nameof(toyCar)} info: {toyCar.Info()}");
        
        
        var font = assetManager.AddFont(AssemblyPath.Of(Enjune.Enjune.Assembly, "Fonts", "papyrus.ttf"), 128, out error);
        if (font == null) return error;

        var assets = assetManager.Compile();

        var grapi = new OpenGlApi().Init(assets, InitialWindowSize, "SceneMaker", _inputHandler, out error);
        if (grapi == null) return error;
        _grapi = grapi;
        _grapi.SetVsync(false);
        _grapi.SetClearColor(new Color(0.2f, 0.2f, 0.2f, 0f));
        _grapi.SetCursorMode(IGraphicApi.CursorMode.Centered);
        
        _wasdController = new FlyingPlayerController(_grapi, _inputHandler, _wasd, 0.2f);
        
        _editorController = new EditorController(_grapi, _inputHandler, _scene);
        
        {
            {
                var m = new Model.Builder()
                    .Add(Mesh.Cube(Position.Zero, 0.5f, TextureQuad.Full), new Model.PerMesh(assetManager.WhiteMaterial))
                    .Build();
                var light = new SObject()
                {
                    Model = m,
                    RenderableModel = _grapi.CreateStaticRenderable(m),
                    Position = (0, 20, -25/2f),
                    PointLight = SpotLight.Ortho(new Vector3(0.3f, -1, 0.3f), new Color(244/255f, 233/255f, 155/255f, 1f)*1.5f, (30, 30))
                };
                _scene.Objects.Add(light);
            }

            {
                var m = new Model.Builder()
                    .Add(Mesh.Cube(Position.Zero, 0.5f, TextureQuad.Full), new Model.PerMesh(assetManager.WhiteMaterial))
                    .Build();
                _scene.Objects.Add(new SObject()
                {
                    Model = m,
                    RenderableModel = _grapi.CreateStaticRenderable(m),
                    Position = (6, 4, 0),
                    PointLight = SpotLight.Perspective(-Vector3.UnitY, new Color(1, 1, 0, 1), 45f)
                });  
            }
        }
        
        _scene.Objects.Add(new SObject()
        {
            Model = calavera,
            RenderableModel = _grapi.CreateStaticRenderable(calavera)
        });
        
        _scene.Objects.Add(_editorController.AxisObject);

        _fpsUiElement = new UiText(Anchor.FixedAt(0, 1), new Margin(0, 10, 10, -40), 1, font, "fps");
        _ui = new Ui(
            InitialWindowSize,
            _fpsUiElement
        );
        
        _uiModel = _grapi.CreateDynamicRenderable(_ui.CreateModel()); 
        
        return null;
    }


    public void MainCycle()
    {
        // var eventThread = new Thread(() =>
        // {
        //     //_grapi.SetCurrentThreadToRender();
        //     //GraphicCycle();
        //     Utils.RunTargetFpsLoopWhile(200,
        //         () => !_grapi.ShouldStop(),
        //         _ => _grapi.UpdateEvents());
        // });
        // eventThread.Start();
        // GraphicCycle();
        //
        // eventThread.Join();
        GraphicCycle();
    }

    public void GraphicCycle()
    {
        var projection = Matrix4.CreatePerspectiveFieldOfView(
            MathF.PI / 2, (float) _inputHandler.WindowSize.X / _inputHandler.WindowSize.Y, 0.1f, 100f);
        var fpsStopWatch = Stopwatch.StartNew();

        Utils.RunTargetFpsLoopWhile(500,
            () => !_grapi.ShouldStop(),
            deltaTime =>
            {
                var mouseInputs = _inputHandler.MouseUpdates;
                _inputHandler.PrepareAtFrameStart();
                
                _wasdController.Update(deltaTime);

                bool uiChanged = false;
                if (_inputHandler.DeltaWheelScroll.Y != 0)
                {
                    uiChanged = true;
                    _ui.PixelsPerUnit += _inputHandler.DeltaWheelScroll.Y * 0.1f;
                    _ui.UpdateEntire();
                }
                if (_inputHandler.WindowSizeChanged)
                {
                    _grapi.SetRenderSize(_inputHandler.WindowSize);
                    
                    _ui.Size = _inputHandler.WindowSize;
                    _ui.UpdateEntire();
                    uiChanged = true;
                    
                    projection = Matrix4.CreatePerspectiveFieldOfView(
                        MathF.PI / 2, (float) _inputHandler.WindowSize.X / _inputHandler.WindowSize.Y, 0.1f, 100f);
                }
                
                if (fpsStopWatch.ElapsedMilliseconds > 1000)
                {
                    fpsStopWatch.Restart();
                    //_fpsUiElement.Text = "fps: " + (1f / deltaTime).ToString("0.00");
                    _fpsUiElement.Text = $"mi: {mouseInputs}; fps: {1f / deltaTime:0.00}";
                    _fpsUiElement.UpdateMeshes();
                    uiChanged = true;
                }

                if (uiChanged){
                    _uiModel.Refit(_ui.CreateModel());
                }
                
                var view = _wasdController.View;
                
                if (_inputHandler.IsPressed(_freeCursorBind))
                    _grapi.SetCursorMode(IGraphicApi.CursorMode.Normal);
                else if (_inputHandler.IsPressed(_lockCursorBind))
                    _grapi.SetCursorMode(IGraphicApi.CursorMode.Centered);
                
                if (_inputHandler.IsPressed(_dumbTexturesBind)) 
                    _grapi.DumpTextures(ExternalPath.Of("."));
                
                _editorController.Update(view, projection);
                
                _inputHandler.ClearForNextFrame();
                // render
                
                var lights = _scene.Objects
                    .Where(o => o.PointLight is not null)
                    .Select(o =>
                    {
                        o.PointLight!.Position = o.Position;
                        o.PointLight!.UpdateView();
                        return o.PointLight!;
                    });
                    
                _grapi.SetLights(lights);
                
                _grapi.UseShader<IShader.IShadowMap>(s =>
                {
                    s.ForEachLight(() =>
                    {
                        _grapi.ClearRenderBuffer();
                        foreach (var obj in _scene.Objects)
                        {
                            if (!obj.IsRealistic) continue;
                            if (obj.Hidden) continue;
                            if (obj.RenderableModel is null) continue;
                            if (obj.PointLight is not null) continue;
                            
                            s.ModelTransform(obj.ModelTransform);
                            obj.RenderableModel.Render(s);
                        }
                    });
                });
                
                _grapi.UseShader<IShader.ICamera.IMaterial>(s =>
                {
                    _grapi.ClearRenderBuffer();
                    s.ProjectionTransform(projection);
                    s.ViewTransform(view);
                    s.ViewPosition(_wasdController.Position);
                    
                    foreach (var obj in _scene.Objects)
                    {
                        if (!obj.IsRealistic) continue;
                        if (obj.Hidden) continue;
                        if (obj.RenderableModel is null) continue;
                    
                        if (obj == _editorController.SelectedObject)
                            s.GlobalColor((1, 0.5f, 0f, 1f));
                        else
                            s.GlobalColor(new Color(1f));
                    
                        s.ModelTransform(obj.ModelTransform);
                        obj.RenderableModel.Render(s);
                    }
                });
                
                _grapi.UseShader<IShader.ICamera.IColor>(s =>
                {
                    // drawing everything else over
                    _grapi.ClearRenderBuffer(false, true);
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
                    
                    _grapi.ClearRenderBuffer(false, true);
                    s.ModelTransform(_ui.ModelTransform);
                    s.ViewTransform(_ui.ViewTransform);
                    s.ProjectionTransform(_ui.ProjectionTransform);
                    
                    _uiModel.Render(s);
                });
                
                // end
                _grapi.UpdateScreen();

                _grapi.UpdateEvents();
            });
    }

    protected override void DisposeData()
    {
        foreach (var o in _scene.Objects)
        {
            o.RenderableModel?.Dispose();
        }
        
        Utils.DisposeAllFields(this);
    }
}