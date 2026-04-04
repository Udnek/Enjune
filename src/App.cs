using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.GraphicApi.OpenGL;
using Enjune.Graphic.InputHandler;
using Enjune.Misc;
using OpenTK.Mathematics;
using static Enjune.Misc.Misc;

namespace Enjune;

public class App
{
    private int _windowWidth = 480;
    private int _windowHeight = 360;
    
    private readonly IGraphicApi _grapi = new OpenGLApi();
    private readonly BasicInputHandler _inputHandler = new BasicInputHandler();
    private FlyingPlayerController _controller = null!;
    
    private readonly VertexBuffer _vertexBuffer = new VertexBuffer(20_000);
    private readonly List<Model> _models = new();

    private IGraphicApi.DrawMode _drawMode = IGraphicApi.DrawMode.Fill;
    
    private void WindowSizeChangeHandler(int w, int h)
    {
        _windowWidth = w;
        _windowHeight = h;
        _grapi.ViewPort(0, 0, w, h);
    }

    public void Init()
    {
        _controller = new FlyingPlayerController(_inputHandler);
        var textureManager = new AssetManager();

        var model = new DotObjModelReader(textureManager, new ResourcePath("Models", "wt", "wooden watch tower2.obj")).Read(out var error)
            ?? throw new Exception(error);
        Logger.Log(this, $"model info: {model.Info()}");
        
        
        _models.Add(model);
        //Logger.Log(this, $"Kukuruznik model size: {kukuruznikModel.Vertices.Length}");
        var assets = textureManager.Compile();

        _grapi.Init(assets, _windowWidth, _windowHeight, "Enjune C#", _inputHandler, WindowSizeChangeHandler);
        _grapi.SetClearColor(new Color(0.1f, 0.1f, 0.1f, 1f));
        
        _vertexBuffer.Clear();
        foreach (var m in _models)
        {
            _vertexBuffer.PutModel(m);
        }
        //_vertexBuffer.PutMesh(Mesh.Cube(new Position(0, 0, -4f), 2, TextureQuad.Full), 0);
    }
    
    public void Run()
    {
        var delays = new List<long>(200);
        float deltaTime = 0;
        RunTargetFpsLoopWhile(144,
            out deltaTime,
            (delay) =>
            {
                delays.Add(delay);
            },
            () => !_grapi.ShouldStop(),
            () =>
            {
                if (_inputHandler.IsPressed(KeyBinds.DumpTextures)) 
                    _grapi.DumpTextures();
                if (_inputHandler.IsPressed(KeyBinds.SwitchDrawMode))
                {
                    _drawMode = (IGraphicApi.DrawMode)((int)(_drawMode + 1) % Enum.GetValues(typeof(IGraphicApi.DrawMode)).Length);
                    _grapi.SetDrawMode(_drawMode);
                }
                
                
                _controller.Update(deltaTime);
                _grapi.Projection(Matrix4.CreatePerspectiveFieldOfView(
                    MathF.PI / 2, (float) _windowWidth / _windowHeight, 0.1f, 1000f));
                
                _grapi.View(_controller.View);
                
                _inputHandler.ClearForNextFrame();
                
                _grapi.ClearScreenBuffers();
                _grapi.RenderToScreenBuffer(_vertexBuffer);
                _grapi.UpdateScreen();
                _grapi.UpdateEvents();
            });
        
        var avgDelay = delays.Sum(v => v) / delays.Count;
        Console.WriteLine($"Avg delay: {avgDelay}; avg possible fps: {NanoDelayToFps(avgDelay)}");
        _grapi.Dispose();
    }
}