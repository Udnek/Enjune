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
using Minecraft2.Block;
using Minecraft2.Ecs;
using Minecraft2.Ecs.Component;
using Minecraft2.Ecs.System;
using Minecraft2.Graphic;
using Minecraft2.Misc;
using Minecraft2.Terrain;
using OpenGLApi;
using OpenTK.Mathematics;

namespace Minecraft2;

public class App : AbstractDisposable, IApp
{
    private static readonly Vector2i InitialWindowSize = (480*2, 360*2);
    private const string Title = "Minecraft2";

    #region Public
    
    private IGraphicApi _graphicApi = null!;
    public GraphicEngine GraphicEngine { get; private set; } = null!;
    public readonly BasicInputHandler InputHandler;
    public FlyingPlayerController FlyingController { get; private set; } = null!;
    public readonly KeyBinds Binds;
    public World World { get; private set; } = null!;
    public ChunkWorld ChunkWorld { get; private set; } = null!;
    public Entity PlayerEntity { get; private set; }
    public RecyclingPool<ChunkModel> ChunkModelPool { get; private set; } = new(() => new ChunkModel());
    public RecyclingPool<Chunk> ChunkPool { get; private set; }
    public BaseTerrainGenerator TerrainGenerator { get; private set; } = null!;
    public ChunkMeshGenerator ChunkMeshGenerator { get; private set; } = null!;
    public bool IsRunning { get; private set; } = true;
    public BlockTypes Blocks { get; private set; } = null!;
    
    #endregion
    
    private readonly Wasd _wasd;
    private readonly KeyBinds.Bind _freeCursorBind;
    private readonly KeyBinds.Bind _lockCursorBind;

    public App()
    {
        Binds = KeyBinds.CreateEmpty();
        _wasd = Wasd.AddTo(Binds);
        
        InputHandler = new BasicInputHandler(InitialWindowSize, 0.5f, () => IsRunning = false);

        _freeCursorBind = Binds.AddBind(new KeyBinds.Bind("free_cursor", KeyCode.Escape));
        _lockCursorBind = Binds.AddBind(new KeyBinds.Bind("lock_cursor", KeyCode.RightMouseButton));

        ChunkPool = new RecyclingPool<Chunk>(() => new Chunk(
            ChunkPos.Zero,
            new ChunkLayer<BlockType>(ChunkPos.Zero, Blocks.Air)));
    }

    public Error? Init()
    {
        // components
        Components.Boot();
        
        // assets
        var taal = new TextureArrayAssetLoader();
        Blocks = new BlockTypes(taal);
        if (taal.Compile().GetIsFailed(out var blockAssets, out var error))
            return "can not compile block assets: " + error;

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
            Speed = 60,
            Position = (Chunk.Size.X/2f, 50, Chunk.Size.X/2f)
        };
        
        // world load
        {
            World = new World();
            ChunkWorld = new ChunkWorld(this);
            PlayerEntity = World.AddEntity(new Entity.Assembly()
                .AddComponent(new ChunkLoader
                {
                    HorizontalRadius = 8,
                    VerticalRadius = 3
                })
                .AddComponent(new Transform()));

            Systems.AddTo(World, this);
        }
        
        ChunkMeshGenerator = new ChunkMeshGenerator(this);
        TerrainGenerator = new BaseTerrainGenerator(this);
        
        // starting workers
        TerrainGenerator.Start("TerrainGenerator");
        ChunkMeshGenerator.Start("MeshGenerator");
        
        return null;
    }

    public void MainCycle()
    {
        var logicThread = new Thread(() =>
        {
            Utils.RunTargetFpsLoopWhile(
                20, 
                () => IsRunning,
                LogicCycle
            ); 
        });
        logicThread.Name = "Logic";
        logicThread.Start();

        Utils.RunTargetFpsLoopWhile(
            200, 
            () => IsRunning,
            GraphicCycle
            );

        logicThread.Join();
        
        TerrainGenerator.Stop();
        ChunkMeshGenerator.Stop();
    }

    private void LogicCycle(float deltaTime)
    {
        World.ModifyEntityComponent<Transform>(PlayerEntity, transform =>
        {
            transform.Position = FlyingController.Position;
            return transform;
        });
        
        if (InputHandler.IsLongPressed(KeyCode.P))
        {
            Logger.Highlight(this, $"Look dir: {FlyingController.Direction}");
        }

        if (InputHandler.IsLongPressed(KeyCode.LeftMouseButton))
        {
            if (ChunkWorld.TryTraceBlock(FlyingController.Position, FlyingController.Direction, 10,
                    out BlockPos hitPos, out _, out var hitChunk, out _))
            {
                Logger.Highlight(this, $"break at {hitPos}");
                hitChunk.BlockLayer.SetByGlobal(hitPos, Blocks.Air);
            }
        }
        
        if (InputHandler.IsLongPressed(KeyCode.RightMouseButton))
        {
            if (ChunkWorld.TryTraceBlock(FlyingController.Position, FlyingController.Direction, 10,
                    out BlockPos hitPos, out _, out var hitChunk,  out var hitFace))
            {
                if (hitFace != null)
                {
                    var placePos = hitPos + hitFace.Value.Direction();
                    Logger.Highlight(this, $"place at {placePos}, {hitFace}");
                    hitChunk.BlockLayer.SetByGlobal(placePos, Blocks.Cobblestone);
                }
            }
        }
        
        // world
        World.Update();
    }

    private void GraphicCycle(float deltaTime)
    {
        InputHandler.PrepareAtFrameStart();
        
        // wasd
        FlyingController.Update(deltaTime);
        
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