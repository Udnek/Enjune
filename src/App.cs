using Enjune.Graphic;
using Enjune.Graphic.InputHandler;
using Enjune.Graphic.OpenGL;
using OpenTK.Mathematics;
using static Enjune.Misc.Misc;

namespace Enjune;

public class App
{
    private int _windowWidth = 480;
    private int _windowHeight = 360;
    private readonly IGraphicApi _grapi = new OpenGLApi();
    
    void WindowSizeChangeHandler(int w, int h)
    {
        _windowWidth = w;
        _windowHeight = h;
        _grapi.ViewPort(0, 0, w, h);
    }
    
    public void Run()
    {
        var keyHandler = new BasicInputHandler();

        _grapi.Init(_windowWidth, _windowHeight, "EngineU C#", keyHandler, WindowSizeChangeHandler);
        _grapi.SetClearColor(new Color(0.1f, 0.1f, 0.1f, 1f));
        
        var polygons = new List<Polygon>();
        var meshes = new List<Mesh>();

        int yOffset = 0;
        // for (int i = 0; i < 10; i++)
        // {
        //     for (int j = 0; j < 10; j++)
        //     {
        //         Polygon.Cube(new Position(0f + i, 0f + j, -4f), 0.7f, polygons.Add);
        //         yOffset = j;
        //     }
        // }
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                meshes.Add(Mesh.Cube(new Position(i, j + yOffset, -4f), 0.9f, AtlasUtils.GetAt(i, j)));
            }
        }
        
        var o = 0f;
        var red = new Color(1f, o, o, 1f);
        var green = new Color(o, 1f, o, 1f);
        var blue = new Color(o, o, 1f, 1f);
        
        _grapi.ClearVerticesBuffers();
        foreach (var mesh in meshes)
        {
            _grapi.PutColoredMesh(mesh, new Color(1,1,1,1));
        }
        foreach (var poly in polygons)
        {
            _grapi.PutWhiteVertex(poly.V0, AtlasUtils.GetAt(8, 0).BotLeft);
            _grapi.PutWhiteVertex(poly.V1, AtlasUtils.GetAt(8, 0).BotRight);
            _grapi.PutWhiteVertex(poly.V2, AtlasUtils.GetAt(8, 0).TopRight);
        }

        var position = new Position(0f, 0f, 0f);
        var pitch = 0f;
        var yaw = 0f;

        var tick = 0;
        
        var delays = new List<long>(200);

        float deltaTime = 1;
        RunTargetFpsLoopWhile(144f,
            (delay) =>
            {
                //deltaTime = NanosToSeconds(delay);
                delays.Add(delay);
                //if (tick % 20 == 0) _grapi.Title($"{NanoDelayToFps(delay)}");
            },
            () => !_grapi.ShouldStop(),
            () =>
            {
                // rotation input
                if (keyHandler.IsPressed(KeyBinds.LookLeft)) yaw += 2;
                if (keyHandler.IsPressed(KeyBinds.LookRight)) yaw -= 2;
                if (keyHandler.IsPressed(KeyBinds.LookUp)) pitch += 2;
                if (keyHandler.IsPressed(KeyBinds.LookDown)) pitch -= 2;
                yaw %= 360;
                pitch = Math.Clamp(pitch, -90f, 90f);

                var radYaw = MathHelper.DegreesToRadians(yaw);
                var radPitch = MathHelper.DegreesToRadians(pitch);
                
                var yawRotation = Matrix4.CreateRotationY(radYaw);
                var pitchRotation = Matrix4.CreateRotationX(radPitch);
                var cameraRotation = yawRotation * pitchRotation;

                // movement input
                var move = new Vector3(0f, 0f, 0f);
                if (keyHandler.IsPressed(KeyBinds.Forward))
                    move += yawRotation.TransformDirection(new Vector3(0f, 0f, -1f));
                else if (keyHandler.IsPressed(KeyBinds.Backward)) 
                    move += yawRotation.TransformDirection(new Vector3(0f, 0f, 1f));

                if (keyHandler.IsPressed(KeyBinds.Rightward))
                    move += yawRotation.TransformDirection(new Vector3(1f, 0f, 0f));
                else if (keyHandler.IsPressed(KeyBinds.Leftward)) 
                    move += yawRotation.TransformDirection(new Vector3(-1f, 0f, 0f));

                if (keyHandler.IsPressed(KeyBinds.Upward))
                    move += new Vector3(0f, 1f, 0f);
                else if (keyHandler.IsPressed(KeyBinds.Downward)) 
                    move += new Vector3(0f, -1f, 0f);
                
                position += move * 0.2f * deltaTime;


                _grapi.Projection(Matrix4.CreatePerspectiveFieldOfView(
                    MathF.PI / 2, ((float) _windowWidth) / _windowHeight, 0.1f, 100f));
                
                // todo figure out even why this shit works
                _grapi.View(Matrix4.CreateTranslation(-position) 
                            * Matrix4.CreateRotationY(-radYaw)
                            * Matrix4.CreateRotationX(-radPitch)
                            );
                
                keyHandler.ClearForNextFrame();
                
                _grapi.ClearScreenBuffers();
                _grapi.RenderToScreenBuffer();
                _grapi.UpdateScreen();
                _grapi.UpdateEvents();
                tick += 1;
            });

        _grapi.Destroy();
        var avgDelay = delays.Sum(v => v) / delays.Count;
        Console.WriteLine($"Avg delay: {avgDelay}; avg possible fps: {NanoDelayToFps(avgDelay)}");
        
    }
}