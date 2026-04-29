using Enjune;
using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Font;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.Input;
using Enjune.Misc;
using Enjune.World;
using OpenGLApi;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SceneMaker;

public class App : AbstractDisposable, IApp
{
    private int _windowWidth = 480*2;
    private int _windowHeight = 360*2;
    
    private readonly IGraphicApi _grapi = new OpenGlApi();
    private readonly KeyBinds _binds;
    private readonly Wasd _wasd;
    private readonly KeyBinds.Bind _dumbTexturesBind;
    
    private readonly BasicInputHandler _inputHandler;
    private readonly FlyingPlayerController _wasdController;

    // private MaterialVertexBuffer _materialVertexBuffer = null!;
    // private ColoredVertexBuffer _colorVertexBuffer = null!;
    
    private readonly KeyBinds.Bind _freeCursorBind;
    private readonly KeyBinds.Bind _lockCursorBind;
    private Scene _scene;
    private EditorController _editorController = null!;

    public App()
    {
        _binds = KeyBinds.CreateEmpty();
        KeyBinds.AddWasd(_binds, out _wasd);
        _freeCursorBind = _binds.AddBind(new KeyBinds.Bind("free_cursor", KeyCode.Escape));
        _lockCursorBind = _binds.AddBind(new KeyBinds.Bind("lock_cursor", KeyCode.RightMouseButton));
        
        _dumbTexturesBind = _binds.AddBind(new KeyBinds.Bind("dumb_textures", KeyCode.F2));
        
        _scene = new Scene();
        
        _inputHandler = new BasicInputHandler(_grapi, _binds);
        _wasdController = new FlyingPlayerController(_grapi, _inputHandler, _wasd, 0.2f);
    }
    
    private void WindowSizeChangeHandler(int w, int h)
    {
        _windowWidth = w;
        _windowHeight = h;
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
        
        error = _grapi.Init(assets, _windowWidth, _windowHeight, "Enjune C#", 42, _inputHandler, WindowSizeChangeHandler);
        if (error != null) return error;
        _grapi.SetVsync(false);
        _grapi.SetClearColor(new Color(0.2f, 0.2f, 0.2f, 0f));
        _grapi.SetCursorMode(IGraphicApi.CursorMode.Centered);
        _grapi.SetVsync(false);
        
        _editorController = new EditorController(_grapi, _inputHandler, _scene);
        
        {
            {
                var m = new Model<(TextureCoord texCoord, Normal normal), CompiledMaterial>.Builder()
                    .Add(Mesh.Cube(Position.Zero, 0.5f, TextureQuad.Full), assetManager.WhiteMaterial)
                    .Build();
                _scene.Objects.Add(new SObject()
                {
                    MatModel = m,
                    RMatModel = _grapi.CompileModel(m),
                    Position = (0, 8, 0),
                    PointLight = new PointLight(Position.Zero, Color.UnitX)
                });
            }

            {
                var m = new Model<(TextureCoord texCoord, Normal normal), CompiledMaterial>.Builder()
                    .Add(Mesh.Cube(Position.Zero, 0.5f, TextureQuad.Full), assetManager.WhiteMaterial)
                    .Build();
                _scene.Objects.Add(new SObject()
                {
                    MatModel = m,
                    RMatModel = _grapi.CompileModel(m),
                    Position = (6, 8, 0),
                    PointLight = new PointLight(Position.Zero, Color.UnitY)
                });  
            }

            {
                var m = new Model<(TextureCoord texCoord, Normal normal), CompiledMaterial>.Builder()
                    .Add(Mesh.Cube(Position.Zero, 0.5f, TextureQuad.Full), assetManager.WhiteMaterial)
                    .Build();
                _scene.Objects.Add(new SObject()
                {
                    MatModel = m,
                    RMatModel = _grapi.CompileModel(m),
                    Position = (-6, 8, 0),
                    PointLight = new PointLight(Position.Zero, Color.UnitZ)
                });
            }
        }
        
        _scene.Objects.Add(new SObject()
        {
            MatModel = calavera,
            RMatModel = _grapi.CompileModel(calavera)
        });
        
        _scene.Objects.Add(_editorController.AxisObject);
        
        return null;
    }

    public void MainCycle()
    {
        var delays = new List<Nanoseconds>(200);
        int tick = 0;
        Seconds deltaTime = 0;
        Utils.RunTargetFpsLoopWhile(200,
            out deltaTime,
            delay =>
            {
                delays.Add(delay);
            },
            () => !_grapi.ShouldStop(),
            () =>
            {
                _wasdController.Update(deltaTime);

                var projection = Matrix4.CreatePerspectiveFieldOfView(
                    MathF.PI / 2, (float) _windowWidth / _windowHeight, 0.1f, 1000f);
                var view = _wasdController.View;
                
                if (_inputHandler.IsPressed(_freeCursorBind))
                    _grapi.SetCursorMode(IGraphicApi.CursorMode.Normal);
                else if (_inputHandler.IsPressed(_lockCursorBind))
                    _grapi.SetCursorMode(IGraphicApi.CursorMode.Centered);
                
                if (_inputHandler.IsPressed(_dumbTexturesBind)) 
                    _grapi.DumpTextures(ExternalPath.Of("."));
                
                _editorController.Update(view, projection);
                
                // render
                _grapi.ClearScreenBuffers();
                
                _grapi.UseShader<IShader.I3D.IMaterial>(s =>
                {
                    s.ProjectionTransform(projection);
                    s.ViewTransform(view);
                    s.ViewPosition(_wasdController.Position);

                    
                    var lights = _scene.Objects
                        .Where(o => o.PointLight is not null)
                        .Select(o =>
                        {
                            o.PointLight!.Position = o.Position;
                            return o.PointLight!;
                        }).ToList();
                    s.Lights(lights);
                    
                    foreach (var obj in _scene.Objects)
                    {
                        if (obj.Hidden) continue;
                        if (obj.RMatModel is null) continue;
                    
                        if (obj == _editorController.SelectedObject)
                            s.GlobalColor((1, 0.5f, 0f, 1f));
                        else
                            s.GlobalColor(new Color(1f));
                    
                        s.ModelTransform(obj.ModelTransform);
                        obj.RMatModel.Render(s);
                    }
                });
                
                // drawing everything else over
                _grapi.ClearScreenBuffers(false, true);
                
                _grapi.UseShader<IShader.I3D.IColor>(s =>
                {
                    s.ProjectionTransform(projection);
                    s.ViewTransform(view);
                    
                    foreach (var obj in _scene.Objects)
                    {
                        if (obj.Hidden) continue;
                        if (obj.RColorModel is null) continue;
                
                        s.ModelTransform(obj.ModelTransform);
                
                        obj.RColorModel.Render(s);
                    }
                });
                
                // end
                _inputHandler.ClearForNextFrame();
                _grapi.UpdateScreen();
                _grapi.UpdateEvents();
                tick += 1;
            });
        
        var avgDelay = delays.Sum(v => v) / delays.Count;
        Console.WriteLine($"Avg delay: {avgDelay}; avg possible fps: {Utils.NanoDelayToFps(avgDelay)}");
        
    }

    protected override void DisposeData()
    {
        foreach (var o in _scene.Objects)
        {
            o.RMatModel?.Dispose();
            o.RColorModel?.Dispose();
        }

        Utils.DisposeAllFields(this);
    }
}