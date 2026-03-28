using Enjune.Graphic;
using Enjune.Graphic.KeyHandler;
using Enjune.Graphic.OpenGL;
using OpenTK.Mathematics;
using static Enjune.Misc.Misc;

namespace Enjune;

public class App
{
    private int _windowWidth = 480;
    private int _windowHeight = 360;
    private readonly IGraphicApi _grapi = new OpenGlApi();
    
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

        var polygons = new List<Polygon>();

        for (int i = 0; i < 300; i++)
        {
            for (int j = 0; j < 300; j++)
            {
                Polygon.Cube(new Position(0f + i, 0f + j, -4f), 0.7f, polygons.Add);
            }
        }

        var o = 0.5f;
        var red = new Color(1f, o, o, 1f);
        var green = new Color(o, 1f, o, 1f);
        var blue = new Color(o, o, 1f, 1f);

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
                _grapi.ClearVerticesBuffers();
                foreach (var poly in polygons)
                {
                    _grapi.PutVertex(poly.V0, red);
                    _grapi.PutVertex(poly.V1, green);
                    _grapi.PutVertex(poly.V2, blue);
                }
                
                // rotation input
                if (keyHandler.IsPressed(KeyBinds.LookLeft)) yaw += 1;
                if (keyHandler.IsPressed(KeyBinds.LookRight)) yaw -= 1;
                if (keyHandler.IsPressed(KeyBinds.LookUp)) pitch += 1;
                if (keyHandler.IsPressed(KeyBinds.LookDown)) pitch -= 1;
                yaw %= 360;
                pitch %= 90;

                var radYaw = MathHelper.DegreesToRadians(yaw);
                var radPitch = MathHelper.DegreesToRadians(pitch);
                
                var yawRotation = Matrix4.CreateRotationY(radYaw);
                var pitchRotation = Matrix4.CreateRotationX(radPitch);
                var cameraRotation = yawRotation * pitchRotation;

                // movement input
                var move = new Vector3(0f, 0f, 0f);
                if (keyHandler.IsPressed(KeyBinds.Forward))
                {
                    move += yawRotation.TransformDirection(new Vector3(0f, 0f, -1f));
                }
                else if (keyHandler.IsPressed(KeyBinds.Backward))
                {
                    move += yawRotation.TransformDirection(new Vector3(0f, 0f, 1f));
                }

                if (keyHandler.IsPressed(KeyBinds.Rightward))
                {
                    move += yawRotation.TransformDirection(new Vector3(1f, 0f, 0f));
                }
                else if (keyHandler.IsPressed(KeyBinds.Leftward))
                {
                    move += yawRotation.TransformDirection(new Vector3(-1f, 0f, 0f));
                }

                if (keyHandler.IsPressed(KeyBinds.Upward))
                {
                    move += new Vector3(0f, 1f, 0f);
                }
                else if (keyHandler.IsPressed(KeyBinds.Downward))
                {
                    move += new Vector3(0f, -1f, 0f);
                }
                
                position += move * 0.2f * deltaTime;


                _grapi.Projection(Matrix4.CreatePerspectiveFieldOfView(
                    MathF.PI / 2, ((float) _windowWidth) / _windowHeight, 0.1f, 100f));

                //var rot4 = new Matrix4(cameraRotation);
                //rot4 = rot4.Transposed();
                //_grapi.View(rot4 * Matrix4.CreateTranslation(-position));
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