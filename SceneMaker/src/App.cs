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
    private FlyingPlayerController _wasdController = null!;
    
    private readonly MaterialVertexBuffer _materialVertexBuffer = new MaterialVertexBuffer(20_000);
    private readonly ColoredVertexBuffer _colorVertexBuffer = new ColoredVertexBuffer(20_000);
    //private readonly List<Model> _models = new();

    private IGraphicApi.DrawMode _drawMode = IGraphicApi.DrawMode.Fill;
    private readonly KeyBinds.Bind _freeCursorBind;
    private Scene _scene;
    private readonly EditorController _editorController;

    public App()
    {
        _binds = KeyBinds.CreateEmpty();
        KeyBinds.AddWasd(_binds, out _wasd);
        _freeCursorBind = new KeyBinds.Bind("free_cursor", GlfwKey.Escape);
        _binds.AddBind(_freeCursorBind);
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
        Logger.Log(this, $"{nameof(watchTower)} info: {watchTower.Info()}");

        var font = assetManager.AddFont(AssemblyPath.Of(Enjune.Enjune.Assembly, "Fonts", "papyrus.ttf"), 128, out error);
        if (font == null) return error;

        var assets = assetManager.Compile();

        error = _grapi.Init(assets, _windowWidth, _windowHeight, "Enjune C#", _inputHandler, WindowSizeChangeHandler);
        if (error != null) return error;
        _grapi.SetClearColor(new Color(0.2f, 0.2f, 0.2f, 0f));
        
        _grapi.SetCursorMode(IGraphicApi.CursorMode.Centered);
        
        _scene.Objects.Add(new SObject(watchTower));
        _scene.Objects.Add(new SObject(font.Generate("Niggers", 10f), true));
        
        return null;
    }

    public void Run()
    {
        //var pixelBuffer = new FixedBuffer<Vector2>(9999999);

        var gizmo = new Model<Color, Color>.Builder().Add(
            new Mesh<Color>([(0, 0, 0), (1, 0, 0), (0, 1, 0), (0, 0, 1)], 
                [Color.One, Color.UnitX, Color.UnitY, Color.UnitZ], 
                [0, 1,    0, 2,  0, 3]),
            Color.One
        ).Build();

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
                
                foreach (var sh in Enum.GetValues<IGraphicApi.ShaderType>())
                {
                    _grapi.SwitchShader(sh);
                    _grapi.ProjectionTransform(projection);
                    _grapi.ViewTransform(view);
                }
                
                _editorController.Update(view, projection);
                
                // other inputs
                if (_inputHandler.IsPressed(_freeCursorBind))
                    _grapi.SetCursorMode(IGraphicApi.CursorMode.Normal);
                
                if (_inputHandler.IsPressed(_dumbTexturesBind)) 
                    _grapi.DumpTextures(ExternalPath.Of("."));
                
                // render
                
                _grapi.ClearScreenBuffers();
                
                foreach (var sObject in _scene.Objects)
                {
                    _grapi.SwitchShader(IGraphicApi.ShaderType.Main);
                    _materialVertexBuffer.Clear();
                    _materialVertexBuffer.PutModel(sObject.Model);
                    
                    _grapi.ModelTransform(sObject.ModelMatrix);
                    
                    if (sObject == _editorController.SelectedObject)
                    {
                        _grapi.GlobalColor((1, 0.5f, 0f, 1f));
                    }
                    else
                    {
                        _grapi.GlobalColor(new Color(1f));
                    }
                    
                    _grapi.RenderToScreenBuffer(_materialVertexBuffer);
                    // _grapi.SetDrawMode(IGraphicApi.DrawMode.Wireframe);
                    // _grapi.RenderToScreenBuffer(_colorVertexBuffer);
                }
                
                // gizmo
                _grapi.ClearScreenBuffers(false, true);
                _colorVertexBuffer.Clear();
                _colorVertexBuffer.PutModel(gizmo);
                _grapi.SwitchShader(IGraphicApi.ShaderType.Color);
                _grapi.ModelTransform(Matrix4.CreateTranslation(0, 0, 0));
                _grapi.ViewTransform(Matrix4.LookAt(-_wasdController.Direction*3, (0, 0, 0), Vector3.UnitY));
                _grapi.RenderToScreenBuffer(_colorVertexBuffer, IGraphicApi.Primitive.Line);
                
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