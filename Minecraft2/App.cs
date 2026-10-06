using System.Resources;
using Enjune;
using Enjune.Attribute;
using Enjune.Ecs;
using Enjune.Ecs.EcsType;
using Enjune.File;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Key;
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
    private static readonly string Title = "Minecraft2";

    #region Public

    [DisposeAtLast("other objects may cause segfault when disposing //TODO fix")] // TODO probably dispose all GlModels in GlApi itself?
    public IGraphicApi GraphicApi { get; private set; } = null!;
    public readonly GraphicEngine GraphicEngine;
    public readonly BasicInputHandler InputHandler;
    public FlyingPlayerController WasdController { get; private set; } = null!;
    public readonly KeyBinds Binds;
    public World World { get; private set; } = null!;

    #endregion
    
    private readonly Wasd _wasd;
    private readonly KeyBinds.Bind _dumbTexturesBind;

    public App()
    {
        Binds = KeyBinds.CreateEmpty();
        _wasd = Wasd.AddTo(Binds);
        
        InputHandler = new BasicInputHandler(InitialWindowSize, 0.5f);
        _dumbTexturesBind = Binds.AddBind(new KeyBinds.Bind("dumb_textures", KeyCode.F2));

        GraphicEngine = new GraphicEngine(this);
    }

    public Error? Init()
    {
        // components
        Components.Boot();
        
        var assetManager = new AssetManager();

        // compile assets
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
            GraphicApi.SetClearColor(new Vector4(0.2f, 0.2f, 0.2f, 0f));
            GraphicApi.SetCursorMode(IGraphicApi.CursorMode.Centered);
        }
        
        // controllers
        {
            WasdController = new FlyingPlayerController(GraphicApi, InputHandler, _wasd)
            {
                Sensitivity =  0.2f,
                Speed = 30
            };
        }
        
        // world load
        {
            World = new World([]);
            Systems.AddTo(World, this);
        }
        
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
        
        // world
        World.Update();
        
        // wasd
        WasdController.Update(deltaTime);

        if (InputHandler.WindowSizeChanged)
        {
            GraphicApi.Title($"{Title} ({InputHandler.WindowSize.X}x{InputHandler.WindowSize.Y})");
        }
        
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
        foreach (var o in GraphicEngine.Objects.Values) 
            o.Model.Dispose();
        
        Utils.DisposeAllFields(this);
    }
}