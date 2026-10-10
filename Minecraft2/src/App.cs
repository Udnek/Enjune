using System.Resources;
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

    [DisposeAtLast("other objects may cause segfault when disposing //TODO fix")] // TODO probably dispose all GlModels in GlApi itself?
    public IGraphicApi GraphicApi { get; private set; } = null!;
    public readonly GraphicEngine GraphicEngine;
    public readonly BasicInputHandler InputHandler;
    public FlyingPlayerController FlyingController { get; private set; } = null!;
    public readonly KeyBinds Binds;
    public World World { get; private set; } = null!;
    public CompiledMaterial DirtMaterial;
    public ChunkWorld ChunkWorld { get; private set; } = null!;
    public Entity PlayerEntity { get; private set; }
    public RecyclingPool<DynamicRenderableModel> ChunkModelPool { get; private set; } = null!;
    public readonly BaseTerrainGenerator TerrainGenerator = new BaseTerrainGenerator();
    public readonly ChunkMeshGenerator ChunkMeshGenerator;
    
    #endregion
    
    private readonly Wasd _wasd;
    private readonly KeyBinds.Bind _dumbTexturesBind;
    private readonly KeyBinds.Bind _freeCursorBind;
    private readonly KeyBinds.Bind _lockCursorBind;

    public App()
    {
        Binds = KeyBinds.CreateEmpty();
        ChunkMeshGenerator = new ChunkMeshGenerator(this);
        _wasd = Wasd.AddTo(Binds);
        
        InputHandler = new BasicInputHandler(InitialWindowSize, 0.5f);
        _dumbTexturesBind = Binds.AddBind(new KeyBinds.Bind("dumb_textures", KeyCode.F2));

        _freeCursorBind = Binds.AddBind(new KeyBinds.Bind("free_cursor", KeyCode.Escape));
        _lockCursorBind = Binds.AddBind(new KeyBinds.Bind("lock_cursor", KeyCode.RightMouseButton));
        
        GraphicEngine = new GraphicEngine(this);
    }

    public Error? Init()
    {
        // components
        Components.Boot();
        
        // assets
        var assetManager = new AssetManager();
        DirtMaterial = assetManager.AddMaterialAndGetCompiled(RawMaterial.FromTexture(AssemblyPath.Of(Program.Assembly, "Dirt.png")));
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
            GraphicApi.SetClearColor(new Vector4(135, 206, 235f, 255f)/255f);
            GraphicApi.SetCursorMode(IGraphicApi.CursorMode.Centered);
            
            ChunkModelPool = new RecyclingPool<DynamicRenderableModel>(() => new DynamicRenderableModel(GraphicApi));
        }
        
        // controllers
        FlyingController = new FlyingPlayerController(GraphicApi, InputHandler, _wasd)
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
                .AddComponent(new ChunkLoader{Radius = 16})
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
            () => !GraphicApi.ShouldStop(),
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
            GraphicApi.Title($"{Title} ({InputHandler.WindowSize.X}x{InputHandler.WindowSize.Y})");
        }
        
        // input
        if (InputHandler.IsPressed(_freeCursorBind))
            GraphicApi.SetCursorMode(IGraphicApi.CursorMode.Normal);
        else if (InputHandler.IsPressed(_lockCursorBind))
            GraphicApi.SetCursorMode(IGraphicApi.CursorMode.Centered);
        
        // render
        GraphicEngine.Update();
        
        // keyboard input
        if (InputHandler.IsPressed(_dumbTexturesBind)) 
            GraphicApi.DumpTextures(ExternalPath.Of("."));
        
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