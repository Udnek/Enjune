using Enjune;
using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.GraphicApi.OpenGL;
using Enjune.Graphic.InputHandler;
using Enjune.Misc;
using Enjune.World;
using OpenTK.Mathematics;

namespace SceneMaker;

public class App : IApp
{
    private int _windowWidth = 480*2;
    private int _windowHeight = 360*2;
    
    private readonly IGraphicApi _grapi = new OpenGLApi();
    private readonly KeyBinds _binds;
    private readonly Wasd _wasd;
    
    private readonly BasicInputHandler _inputHandler;
    private FlyingPlayerController _controller = null!;
    
    private readonly VertexBuffer _vertexBuffer = new VertexBuffer(20_000);
    private readonly List<Model> _models = new();

    private IGraphicApi.DrawMode _drawMode = IGraphicApi.DrawMode.Fill;
    private readonly KeyBinds.Bind _freeCursorBind;
    private Scene _scene;

    public App()
    {
        _binds = KeyBinds.CreateEmpty();
        KeyBinds.AddWasd(_binds, out _wasd);
        _freeCursorBind = new KeyBinds.Bind("free_cursor", GlfwKey.Escape);
        _binds.AddBind(_freeCursorBind);
        
        _inputHandler = new BasicInputHandler(_grapi, _binds);
        _controller = new FlyingPlayerController(_grapi, _inputHandler, _wasd, 0.2f);
        
    }
    
    private void WindowSizeChangeHandler(int w, int h)
    {
        _windowWidth = w;
        _windowHeight = h;
        _grapi.ViewPort(0, 0, w, h);
    }
    
    public void Init(out string? error)
    {
        var textureManager = new AssetManager();

        var model = new DotObjModelReader(textureManager, AssemblyPath.Of(Enjune.Enjune.Assembly,"Models", "wt", "wooden watch tower2.obj"))
            .Read(out error);
        if (model == null) return;

        model.Meshes[0].Item2.Raw.Color = (1, 1, 1, 1);
        Logger.Log(this, $"model info: {model.Info()}");
        _models.Add(model);
        
        var assets = textureManager.Compile();

        _grapi.Init(assets, _windowWidth, _windowHeight, "Enjune C#", _inputHandler, WindowSizeChangeHandler);
        _grapi.SetClearColor(new Color(0.2f, 0.2f, 0.2f, 0f));
        
        _grapi.SetCursorMode(IGraphicApi.CursorMode.Centered);

        _scene = new Scene();
        _scene.Objects.Add(new SObject(model));
        // _vertexBuffer.Clear();
        // foreach (var m in _models)
        // {
        //     _vertexBuffer.PutModel(m);
        // }
    }

    public void Run()
    {
        var delays = new List<long>(200);
        float deltaTime = 0;
        Utils.RunTargetFpsLoopWhile(100,
            out deltaTime,
            (delay) =>
            {
                delays.Add(delay);
            },
            () => !_grapi.ShouldStop(),
            () =>
            {
                
                // if (_inputHandler.IsPressed(KeyBinds.DumpTextures)) 
                //     _grapi.DumpTextures();
                // if (_inputHandler.IsPressed(KeyBinds.SwitchDrawMode))
                // {
                //     _drawMode = (IGraphicApi.DrawMode)((int)(_drawMode + 1) % Enum.GetValues(typeof(IGraphicApi.DrawMode)).Length);
                //     _grapi.SetDrawMode(_drawMode);
                // }
 
                
                if (_inputHandler.IsPressed(_freeCursorBind))
                    _grapi.SetCursorMode(IGraphicApi.CursorMode.Normal);
                
                
                _controller.Update(deltaTime);
                _grapi.ProjectionTransform(Matrix4.CreatePerspectiveFieldOfView(
                    MathF.PI / 2, (float) _windowWidth / _windowHeight, 0.1f, 1000f));
                
                _grapi.ViewTransform(_controller.View);
                
                // render
                
                _grapi.ClearScreenBuffers();
                
                foreach (var sObject in _scene.Objects)
                {
                    sObject.Position.X += 1f*deltaTime;
                    sObject.Rotation *= Quaternion.FromAxisAngle(Vector3.UnitZ, 1*deltaTime);
                    _vertexBuffer.Clear();
                    _vertexBuffer.PutModel(sObject.Model);
                    _grapi.ModelTransform(Matrix4.CreateFromQuaternion(sObject.Rotation) * Matrix4.CreateTranslation(sObject.Position));
                    _grapi.RenderToScreenBuffer(_vertexBuffer);
                }
                
                // end
                _inputHandler.ClearForNextFrame();
                _grapi.UpdateScreen();
                _grapi.UpdateEvents();
            });
        
        var avgDelay = delays.Sum(v => v) / delays.Count;
        Console.WriteLine($"Avg delay: {avgDelay}; avg possible fps: {Utils.NanoDelayToFps(avgDelay)}");
        _grapi.Dispose();
    }
}