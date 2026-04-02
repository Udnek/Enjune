using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Graphic;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.InputHandler;
using Enjune.Graphic.OpenGL;
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
    
    private readonly WhiteVertexBuffer _vertexBuffer = new WhiteVertexBuffer(20_000);
    private readonly List<Mesh> _meshes = new();

    private void WindowSizeChangeHandler(int w, int h)
    {
        _windowWidth = w;
        _windowHeight = h;
        _grapi.ViewPort(0, 0, w, h);
    }

    public void Init()
    {
        _controller = new FlyingPlayerController(_inputHandler);
        var textureManager = new TextureManager();

        new DotObjModelReader(textureManager, new ResourcePath("Models", "test.obj")).Read(_meshes.Add, out var error);
        if (error != null) throw new Exception(error);
        
        foreach (var mesh in _meshes) _vertexBuffer.PutMesh(mesh);
        
        //Logger.Log(this, $"Kukuruznik model size: {kukuruznikModel.Vertices.Length}");
        
        _grapi.Init(textureManager, _windowWidth, _windowHeight, "Enjune C#", _inputHandler, WindowSizeChangeHandler);
        _grapi.SetClearColor(new Color(0.1f, 0.1f, 0.1f, 1f));
    }
    
    public void Run()
    {
        var random = new Random();
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
                _vertexBuffer.Clear();
                foreach (var mesh in _meshes) _vertexBuffer.PutMesh(mesh);
                
                _controller.Update(deltaTime);
                _grapi.Projection(Matrix4.CreatePerspectiveFieldOfView(
                    MathF.PI / 2, ((float) _windowWidth) / _windowHeight, 0.1f, 1000f));
                
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