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
        
        // int yOffset = 0;
        // for (int i = 0; i < 16; i++)
        // {
        //     for (int j = 0; j < 16; j++)
        //     {
        //         _meshes.Add(Mesh.Cube(new Position(i, j + yOffset, -4f), 0.9f, TextureQuad.GetAt(i, j)));
        //     }
        // }
        // var poses = new List<Position>();
        // poses.Add(new Position(0, 0, -4));
        // var angle = 0f;
        // for (int i = 0; i < 6; i++)
        // {
        //     poses.Add(new Position(MathF.Cos(MathHelper.DegreesToRadians(angle))*2, MathF.Sin(MathHelper.DegreesToRadians(angle))*2, -4));
        //     angle += 30;
        // }
        // _meshes.Add(Mesh.Ngon(poses.ToArray(), TextureQuad.Furnace));
        var kukuruznikModel = new ObjModelReader(new ResourcePath("Models", "power_lines.obj")).Mesh;
        _meshes.Add(kukuruznikModel);
        
        foreach (var mesh in _meshes) _vertexBuffer.PutMesh(mesh);
        
        Logger.Log($"Kukuruznik model size: {kukuruznikModel.Vertices.Length}");
        
        _grapi.Init(_windowWidth, _windowHeight, "Enjune C#", _inputHandler, WindowSizeChangeHandler);
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
                //deltaTime = NanosToSeconds(delay);
                //delays.Add(delay);
                //if (tick % 20 == 0) _grapi.Title($"{NanoDelayToFps(delay)}");
            },
            () => !_grapi.ShouldStop(),
            () =>
            {
                // var index = random.Next(_meshes[0].Vertices.Length);
                // var vertex = _meshes[0].Vertices[index];
                // var offset = (random.NextSingle() - 0.5f)*5;
                // switch (random.Next(3))
                // {
                //     case 0:
                //         vertex.X += offset;
                //         break;
                //     case 1:
                //         vertex.Y += offset;
                //         break;
                //     case 2:
                //         vertex.Z += offset;
                //         break;
                // }
                //
                // _meshes[0].Vertices[index] = vertex;
                
                _vertexBuffer.Clear();
                foreach (var mesh in _meshes) _vertexBuffer.PutMesh(mesh);
                
                
                //Logger.Log(deltaTime);
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

        _grapi.Destroy();
        //var avgDelay = delays.Sum(v => v) / delays.Count;
        //Console.WriteLine($"Avg delay: {avgDelay}; avg possible fps: {NanoDelayToFps(avgDelay)}");
        
    }
}