using Enjune;
using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Font;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.GraphicApi.OpenGL;
using Enjune.Graphic.GraphicApi.Vertex.Colored;
using Enjune.Graphic.GraphicApi.Vertex.Material;
using Enjune.Graphic.Input;
using Enjune.Misc;
using Enjune.World;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace SceneMaker;

public class App : IApp
{
    private int _windowWidth = 480*2;
    private int _windowHeight = 360*2;
    
    private readonly IGraphicApi _grapi = new OpenGlApi();
    private readonly KeyBinds _binds;
    private readonly Wasd _wasd;
    private readonly KeyBinds.Bind _dumbTexturesBind;
    
    private readonly BasicInputHandler _inputHandler;
    private readonly FlyingPlayerController _wasdController;

    private MaterialVertexBuffer _materialVertexBuffer = null!;
    private ColoredVertexBuffer _colorVertexBuffer = null!;

    private IGraphicApi.DrawMode _drawMode = IGraphicApi.DrawMode.Fill;
    private readonly KeyBinds.Bind _freeCursorBind;
    private readonly KeyBinds.Bind _lockCursorBind;
    private Scene _scene;
    private readonly EditorController _editorController;

    public App()
    {
        _binds = KeyBinds.CreateEmpty();
        KeyBinds.AddWasd(_binds, out _wasd);
        _freeCursorBind = _binds.AddBind(new KeyBinds.Bind("free_cursor", GlfwKey.Escape));
        _lockCursorBind = _binds.AddBind(new KeyBinds.Bind("lock_cursor", MouseButton.Right));
        
        _dumbTexturesBind = _binds.AddBind(new KeyBinds.Bind("dumb_textures", GlfwKey.F2));
        
        _scene = new Scene();
        
        _inputHandler = new BasicInputHandler(_grapi, _binds);
        _wasdController = new FlyingPlayerController(_grapi, _inputHandler, _wasd, 0.2f);
        _editorController = new EditorController(_grapi, _inputHandler, _scene);
    }
    
    private void WindowSizeChangeHandler(int w, int h)
    {
        _windowWidth = w;
        _windowHeight = h;
        _grapi.ViewPort(0, 0, w, h);
    }
    
    public Error? Init()
    {
        var assetManager = new AssetManager();

        var watchTower 
            = new DotObjModelReader(assetManager, AssemblyPath.Of(Enjune.Enjune.Assembly,"Models", "wt", "wooden watch tower2.obj"))
            .Read(out var error);
        if (watchTower == null) return error;
        watchTower.Meshes[0].Item2.Raw.Color = (1, 1, 1, 1);
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
        
        
        _scene.Objects.Add(new SObject()
        {
            MatModel = new Model<(TextureCoord texCoord, Normal normal), CompiledMaterial>.Builder()
                .Add(Mesh<(TextureCoord texCoord, Normal normal)>.Cube(Position.Zero, 0.5f, TextureQuad.Full), assetManager.WhiteMaterial)
                .Build(),
            Position = (0, 10, 0),
            PointLight = new PointLight(Position.Zero, Color.UnitX)
        });
        
        _scene.Objects.Add(new SObject()
        {
            MatModel = new Model<(TextureCoord texCoord, Normal normal), CompiledMaterial>.Builder()
                .Add(Mesh<(TextureCoord texCoord, Normal normal)>.Cube(Position.Zero, 0.5f, TextureQuad.Full), assetManager.WhiteMaterial)
                .Build(),
            Position = (6, 8, 0),
            PointLight = new PointLight(Position.Zero, Color.UnitY)
        });
        
        _scene.Objects.Add(new SObject()
        {
            MatModel = new Model<(TextureCoord texCoord, Normal normal), CompiledMaterial>.Builder()
                .Add(Mesh<(TextureCoord texCoord, Normal normal)>.Cube(Position.Zero, 0.5f, TextureQuad.Full), assetManager.WhiteMaterial)
                .Build(),
            Position = (-6, 8, 0),
            PointLight = new PointLight(Position.Zero, Color.UnitZ)
        });
        
        
        var toyCar = new DotGlbReader(assetManager, AssemblyPath.Of(Enjune.Enjune.Assembly, "Models", "ToyCar.glb"))
            .Read(out error);
        if (toyCar == null) return error;
        _scene.Objects.Add(new SObject(toyCar)
        {
            Scale = new Vector3(0.05f),
            Rotation = Quaternion.FromEulerAngles(MathHelper.DegreesToRadians(90), 0, 0)
        });

        Logger.Log(this, $"{nameof(toyCar)} info: {toyCar.Info()}");

        
        var font = assetManager.AddFont(AssemblyPath.Of(Enjune.Enjune.Assembly, "Fonts", "papyrus.ttf"), 128, out error);
        if (font == null) return error;

        var assets = assetManager.Compile();

        var verticesCapacity = (int)Math.Pow(2, 20);
        _materialVertexBuffer = new MaterialVertexBuffer(verticesCapacity);
        _colorVertexBuffer = new ColoredVertexBuffer(verticesCapacity);
        
        error = _grapi.Init(assets, _windowWidth, _windowHeight, "Enjune C#", verticesCapacity, _inputHandler, WindowSizeChangeHandler);
        if (error != null) return error;
        _grapi.SetVsync(false);
        _grapi.SetClearColor(new Color(0.2f, 0.2f, 0.2f, 0f));
        
        _grapi.SetCursorMode(IGraphicApi.CursorMode.Centered);
        
        //_scene.Objects.Add(new SObject(font.Generate("Niggers", 10f), true));
        
        return null;
    }

    public void Run()
    {
        _scene.Objects.Add(_editorController.AxisObject);

        var delays = new List<long>(200);
        int tick = 0;
        float deltaTime = 0;
        Utils.RunTargetFpsLoopWhile(100,
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
                
                _grapi.UseShader<IShader.IMaterial>(s =>
                {
                    s.ProjectionTransform(projection);
                    s.ViewTransform(view);
                    s.ViewPosition(_wasdController.Position);

                    _scene.Objects.ForEach(o => o.PointLight?.Position = o.Position);
                    
                    var lights = _scene.Objects
                        .Where(o => o.PointLight is not null)
                        .Select(o => o.PointLight!).ToList();
                    s.Lights(lights);
                    
                    foreach (var obj in _scene.Objects)
                    {
                        if (obj.Hidden) continue;
                        if (obj.MatModel is null) continue;

                        s.ModelTransform(obj.ModelMatrix);
                    
                        _materialVertexBuffer.Clear();
                        _materialVertexBuffer.PutModel(obj.MatModel);
                    
                        if (obj == _editorController.SelectedObject)
                            s.GlobalColor((1, 0.5f, 0f, 1f));
                        else
                            s.GlobalColor(new Color(1f));
                    
                        s.RenderToScreenBuffer(_materialVertexBuffer);
                    }
                });
                
                // drawing everything else over
                _grapi.ClearScreenBuffers(false, true);
                
                _grapi.UseShader<IShader.IColor>(s =>
                {
                    s.ProjectionTransform(projection);
                    s.ViewTransform(view);
                    
                    foreach (var obj in _scene.Objects)
                    {
                        if (obj.Hidden) continue;
                        if (obj.ColorModel is null) continue;

                        s.ModelTransform(obj.ModelMatrix);

                        _colorVertexBuffer.Clear();
                        _colorVertexBuffer.PutModel(obj.ColorModel);
                    
                        s.RenderToScreenBuffer(_colorVertexBuffer, IGraphicApi.Primitive.Line);
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

    public void Dispose()
    {
        _grapi.Dispose();
    }
}