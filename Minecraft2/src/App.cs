using System.Resources;
using Enjune;
using Enjune.Attribute;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.File;
using Enjune.Graphic.Api;
using Enjune.Graphic.Key;
using Enjune.Graphic.Modeling;
using Enjune.KitStart;
using Enjune.KitStart.TAAL;
using Enjune.Misc;
using Minecraft2.Bridge;
using Minecraft2.Ecs;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System;
using Minecraft2.Misc;
using OpenGLApi;
using OpenTK.Mathematics;

namespace Minecraft2;

public class App : AbstractDisposable, IApp
{
    private static readonly Vector2i InitialWindowSize = (480*2, 360*2);
    private const string Title = "Minecraft2";

    #region Public
    
    private IGraphicApi _graphicApi = null!;
    public GraphicEngine GraphicEngine { get; private set; }
    public readonly BasicInputHandler InputHandler;
    public FlyingPlayerController FlyingController { get; private set; } = null!;
    public readonly KeyBinds Binds;
    public World World { get; private set; } = null!;
    public ChunkWorld ChunkWorld { get; private set; } = null!;
    public Entity PlayerEntity { get; private set; }
    public RecyclingPool<ChunkModel> ChunkModelPool { get; private set; } = new(() => new ChunkModel());
    public readonly BaseTerrainGenerator TerrainGenerator = new BaseTerrainGenerator();
    public readonly ChunkMeshGenerator ChunkMeshGenerator;
    public bool IsRunning { get; private set; } = true;

    #endregion
    
    private readonly Wasd _wasd;
    private readonly KeyBinds.Bind _freeCursorBind;
    private readonly KeyBinds.Bind _lockCursorBind;

    public App()
    {
        Binds = KeyBinds.CreateEmpty();
        ChunkMeshGenerator = new ChunkMeshGenerator(this);
        _wasd = Wasd.AddTo(Binds);
        
        InputHandler = new BasicInputHandler(InitialWindowSize, 0.5f, () => IsRunning = false);

        _freeCursorBind = Binds.AddBind(new KeyBinds.Bind("free_cursor", KeyCode.Escape));
        _lockCursorBind = Binds.AddBind(new KeyBinds.Bind("lock_cursor", KeyCode.RightMouseButton));
    }

    public Error? Init()
    {
        // components
        Components.Boot();
        
        // assets
        var taal = new TextureArrayAssetLoader();
        var dirtId = taal.AddMaterialAndGetIndex(RawTAALMaterial.FromTexture(AssemblyPath.Of(Program.Assembly, "Dirt.png")));
        Logger.Highlight(this, $"dirt id: {dirtId}");
        if (taal.Compile().GetIsFailed(out var blockAssets, out var error))
            return "can not create block assets: " + error;
        
        // graphicApi
        {
            var graphicApi = new OpenGlApi().Init(
                InitialWindowSize,
                $"{Title} ({InitialWindowSize.X}x{InitialWindowSize.Y})",
                InputHandler,
                out var graphicError);
            
            if (graphicApi == null) 
                return graphicError;
            _graphicApi = graphicApi;
            
            _graphicApi.SetVsync(false);
            _graphicApi.SetClearColor(new Vector4(135, 206, 235f, 255f)/255f);
            _graphicApi.SetCursorMode(IGraphicApi.CursorMode.Centered);

            
            GraphicEngine = new GraphicEngine(this, _graphicApi);
            error = GraphicEngine.Init(blockAssets);
            if (error != null)
                return "can not init graphics engine: " + error;
        }
        
        // controllers
        FlyingController = new FlyingPlayerController(_graphicApi, InputHandler, _wasd)
        {
            Sensitivity =  0.2f,
            Speed = 30,
            Position = (0, 30, 0)
        };
        
        
        // world load
        {
            World = new World();
            ChunkWorld = new ChunkWorld(this);
            PlayerEntity = World.AddEntity(new Entity.Assembly()
                .AddComponent(new ChunkLoader{Radius = 4})
                .AddComponent(new Transform()));

            Systems.AddTo(World, this);
        }
        
        // starting workers
        TerrainGenerator.Start("TerrainGenerator");
        ChunkMeshGenerator.Start("MeshGenerator");
        
        return null;
    }

    public void MainCycle()
    {
        Utils.RunTargetFpsLoopWhile(
            200, 
            () => IsRunning,
            GraphicCycle
            );
        
        TerrainGenerator.Stop();
        ChunkMeshGenerator.Stop();
    }

    private void GraphicCycle(float deltaTime)
    {
        InputHandler.PrepareAtFrameStart();
        
        // wasd
        FlyingController.Update(deltaTime);
        World.ModifyEntityComponent<Transform>(PlayerEntity, transform =>
        {
            transform.Position = FlyingController.Position;
            return transform;
        });
        
        // world
        //Logger.Highlight(this, "---------------------------------");
        World.Update();
        //Logger.Highlight(this, "---------------------------------");
        
        // window
        if (InputHandler.WindowSizeChanged)
        {
            _graphicApi.SetTitle($"{Title} ({InputHandler.WindowSize.X}x{InputHandler.WindowSize.Y})");
        }
        
        // input
        if (InputHandler.IsPressed(_freeCursorBind))
            _graphicApi.SetCursorMode(IGraphicApi.CursorMode.Normal);
        else if (InputHandler.IsPressed(_lockCursorBind))
            _graphicApi.SetCursorMode(IGraphicApi.CursorMode.Centered);
        
        // render
        GraphicEngine.Update();
        
        // post frame
        InputHandler.ClearForNextFrame();
        _graphicApi.UpdateEvents();
    }

    protected override void DisposeData()
    {
        Utils.DisposeAllFields(this);
    }
}