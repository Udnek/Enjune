using System.Diagnostics;
using Enjune;
using Enjune.Attribute;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.File;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Key;
using Enjune.Graphic.Modeling;
using Enjune.KitStart;
using Enjune.Misc;
using OpenGLApi;
using OpenTK.Mathematics;

namespace ExampleApp;

public class App : AbstractDisposable, IApp 
{
    private static readonly Vector2i InitialWindowSize = (480*2, 360*2);
    private const string Title = "Example";
    
    public IGraphicApi GraphicApi { get; private set; } = null!;
    public readonly BasicInputHandler InputHandler;
    public FlyingPlayerController FlyingController { get; private set; } = null!;
    public readonly KeyBinds Binds;
    private readonly Wasd _wasd;
    public IUniform<CameraUniform> CameraUniform { get; set; }
    public IUniform<TimeUniform> TimeUniform { get; set; }
    public IShader<PerVertex, PerPrimitive> Shader { get; set; }
    
    public App()
    {
        Binds = KeyBinds.CreateEmpty();
        _wasd = Wasd.AddTo(Binds);
        
        InputHandler = new BasicInputHandler(InitialWindowSize, 0.5f);
    }

    public Error? Init()
    {

        // assets
        var assetManager = new AssetManager();
        var assets = assetManager.Compile();

        // graphicApi
        {
            var graphicApi = new OpenGlApi().Init(assets,
                InitialWindowSize,
                $"{Title} ({InitialWindowSize.X}x{InitialWindowSize.Y})",
                InputHandler,
                out var graphicError);
            
            if (graphicApi == null) 
                return graphicError;
            GraphicApi = graphicApi;
            
            GraphicApi.SetVsync(false);
            GraphicApi.SetClearColor(new Vector4(50, 50, 50, 255f)/255f);
            GraphicApi.SetCursorMode(IGraphicApi.CursorMode.Centered);

            var uniformOrError = GraphicApi.CreateUniform<CameraUniform>("camera");
            if (uniformOrError.Error is not null)
                return uniformOrError.Error;
            CameraUniform = uniformOrError.GetOrThrow();
            
            var timeOrError = GraphicApi.CreateUniform<TimeUniform>("time");
            if (timeOrError.Error is not null)
                return timeOrError.Error;
            TimeUniform = timeOrError.GetOrThrow();
            
            var shaderOrError = GraphicApi.CreateShader<PerVertex, PerPrimitive, Varyings>(
                """
                vec4 process(Input input, out Varyings varyings) {
                    varyings.color = input.color;
                    return camera.proj * camera.view * vec4(input.pos, 1);
                }
                """,
                """
                vec4 process(Varyings varyings, PerPrimitive perPrim) {
                    return varyings.color * perPrim.color * (sin(time.sec)+1)/2.0;
                }
                """
                );
            if (shaderOrError.Error is not null)
                return shaderOrError.Error;
            Shader = shaderOrError.GetOrThrow();
        }
        
        // controllers
        FlyingController = new FlyingPlayerController(GraphicApi, InputHandler, _wasd)
        {
            Sensitivity = 0.2f,
            Speed = 3
        };
        
        return null;
    }



    public void MainCycle()
    {
        var mesh = new Mesh<PerVertex, PerPrimitive>(PrimitiveTopology.Triangle);
        mesh.AddTriangle(
            new PerVertex(), 
            new PerPrimitive(),
            (0, 0, 0.5f), (10, 0, 0.5f), (10, 10, 0.5f), TextureQuad.Full);
        mesh.AddTriangle(
            new PerVertex(), 
            new PerPrimitive(){Color = new Vector4(1, 1, 0, 1f)},
            (0, 0, 0.5f), (10, 10, 0.5f), (0, 10, 0.5f), TextureQuad.Full);
        mesh.AddNgon(
            new PerPrimitive(){Color = (0f, 1, 1, 1)},
            new PerVertex(){Position = (0, 10, 0.5f), Color = (1, 0, 0, 1)},
            new PerVertex(){Position = (10, 10, 0.5f), Color = (0, 1, 0, 1)},
            new PerVertex(){Position = (5, 15, 0.5f), Color = (0, 0, 1, 1)}
            );
        mesh.Scale(new Vector3(0.5f));
        
        var meshOrrErr = GraphicApi.CreateRenderable(mesh);
        if (meshOrrErr.Error is not null)
        {
            Logger.Error(this, meshOrrErr.Error);
            return;
        }

        var renderableMesh = meshOrrErr.GetOrThrow();

        var stopwatch = Stopwatch.StartNew();

        Utils.RunTargetFpsLoopWhile(
            200, 
            () => !GraphicApi.ShouldStop(),
            dt =>
            {
                InputHandler.PrepareAtFrameStart();
                
                // wasd
                FlyingController.Update(dt);
        
                // window
                if (InputHandler.WindowSizeChanged)
                {
                    GraphicApi.Title($"{Title} ({InputHandler.WindowSize.X}x{InputHandler.WindowSize.Y})");
                }
                
                // updating uniforms
                CameraUniform.Set(new CameraUniform()
                {
                    Projection = Matrix4.CreatePerspectiveFieldOfView(
                        MathF.PI / 2, (float) InputHandler.WindowSize.X / InputHandler.WindowSize.Y, 0.1f, 1000f),
                    View = FlyingController.View
                });
                TimeUniform.Set(new TimeUniform(){Seconds = (float) stopwatch.Elapsed.TotalSeconds});
        
                // render
                Shader.Render(renderableMesh);
        
                // post frame
                GraphicApi.UpdateScreen();
                GraphicApi.ClearRenderBuffer();
                InputHandler.ClearForNextFrame();
                GraphicApi.UpdateEvents(); 
            });
    }

    protected override void DisposeData()
    {
        Utils.DisposeAllFields(this);
    }
}