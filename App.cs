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
    private IGraphicApi _grapi = new OpenGlApi();
    
    public void Run()
    {
        var keyHandler = new BasicInputHandler();
        _grapi.Init(_windowWidth, _windowHeight, "EngineU C#", keyHandler, (w, h) =>
        {
            _windowWidth = w;
            _windowHeight = h;
            _grapi.ViewPort(0, 0, w, h);
        });

        var polygons = new List<Polygon>();

        for (int i = 0; i < 100; i++)
        {
            Polygon.Cube(new Position(0f, 1f+i, -4f), 3f, polygons.Add);
            Polygon.Cube(new Position(0f, 0f+i, 4f), 0.5f, polygons.Add);
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
        RunTargetFpsLoopWhile(500f,
            (delay) =>
            {
                delays.Add(delay);
                //if (tick % 20 == 0) _grapi.Title($"{NanoDelayToFps(delay)}");
            },
            () => !_grapi.ShouldStop(),
            () =>
            {
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
                
                position += move * 0.2f;
                
                // if (keyHandler.IsPressed(KeyBinds.DebugMenu))
                // {
                //     Console.WriteLine("Pressed menu");
                // }

                foreach (var poly in polygons)
                {
                    _grapi.PutVertex(poly.V0, red);
                    _grapi.PutVertex(poly.V1, green);
                    _grapi.PutVertex(poly.V2, blue);
                }

                _grapi.Projection(Matrix4.CreatePerspectiveFieldOfView(
                    MathF.PI / 2, ((float) _windowWidth) / _windowHeight, 0.1f, 100f));


                //var rot4 = new Matrix4(cameraRotation);
                //rot4 = rot4.Transposed();
                //_grapi.View(rot4 * Matrix4.CreateTranslation(-position));
                _grapi.View(Matrix4.CreateTranslation(-position) 
                            * Matrix4.CreateRotationY(-radYaw)
                            * Matrix4.CreateRotationX(-radPitch)
                            );
                
                _grapi.ClearRenderBuffer();
                _grapi.Render();
                
                keyHandler.ClearForNextFrame();
                _grapi.UpdateEvents();
                tick += 1;
            });

        _grapi.Destroy();
        var avgDelay = delays.Sum(v => v) / delays.Count;
        Console.WriteLine($"Avg delay: {avgDelay}; avg possible fps: {NanoDelayToFps(avgDelay)}");
        
    }
}