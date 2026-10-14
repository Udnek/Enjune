using Enjune;
using Enjune.Attribute;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.File;
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

            var shaderOrError = GraphicApi.CreateShader<PerVertex, PerPrimitive, Varyings>(
                """
                
                """,
                """
                
                """
                );
            if (shaderOrError.Error is not null)
                return shaderOrError.Error;

            var uniformOrError = GraphicApi.CreateUniform<CameraUniform>("camera");
            if (uniformOrError.Error is not null)
                return uniformOrError.Error;
            CameraUniform = uniformOrError.GetOrThrow();
        }
        
        // controllers
        FlyingController = new FlyingPlayerController(GraphicApi, InputHandler, _wasd)
        {
            Sensitivity = 0.2f,
            Speed = 1
        };
        
        return null;
    }

    public void MainCycle()
    {
        Utils.RunTargetFpsLoopWhile(
            200, 
            () => !GraphicApi.ShouldStop(),
            GraphicCycle
            );
    }

    private void GraphicCycle(float deltaTime)
    {
        InputHandler.PrepareAtFrameStart();
        
        // wasd
        FlyingController.Update(deltaTime);
        
        // window
        if (InputHandler.WindowSizeChanged)
        {
            GraphicApi.Title($"{Title} ({InputHandler.WindowSize.X}x{InputHandler.WindowSize.Y})");
        }
        
        // render
        
        
        // post frame
        GraphicApi.UpdateScreen();
        InputHandler.ClearForNextFrame();
        GraphicApi.UpdateEvents();
    }

    protected override void DisposeData()
    {
        Utils.DisposeAllFields(this);
    }
}