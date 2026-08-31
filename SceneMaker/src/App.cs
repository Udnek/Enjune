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
using OpenGLApi;
using OpenTK.Mathematics;
using SceneMaker.Bridge;
using SceneMaker.Ecs.Component;
using SceneMaker.Ecs.System;
using SceneMaker.Misc;
using SceneMaker.Ui;

namespace SceneMaker;

public class App : AbstractDisposable, IApp
{
    private static readonly Vector2i InitialWindowSize = (480*2, 360*2);

    #region Public

    [DisposeAtLast("other objects may cause segfault when disposing //TODO fix")] // TODO probably dispose all GlModels in GlApi itself?
    public IGraphicApi GraphicApi { get; private set; } = null!;
    public readonly GraphicEngine GraphicEngine;
    public readonly BasicInputHandler InputHandler;
    public FlyingPlayerController WasdController { get; private set; } = null!;
    public EditorSystem EditorSystem { get; private set; } = null!;
    public UiManager UiManager { get; private set; } = null!;
    public readonly KeyBinds Binds;

    #endregion
    
    private World _world = null!;
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
        var assetManager = new AssetManager();

        // font
        var font = assetManager.AddFont(AssemblyPath.Of(Enjune.Enjune.Assembly, "Fonts", "vt323.ttf"), 128, out var fontError);
        if (font == null) return fontError;

        // models
        {
            var modelsError = ResourceManager.LoadModels(assetManager);
            if (modelsError != null) return modelsError;
        }
        
        // compile assets
        var assets = assetManager.Compile();

        // graphicApi
        {
            var graphicApi = new OpenGlApi().Init(assets, InitialWindowSize, "Scene Maker", InputHandler, out var graphicError);
            if (graphicApi == null) 
                return graphicError;
            GraphicApi = graphicApi;
        }
        GraphicApi.SetVsync(false);
        GraphicApi.SetClearColor(new Color(0.2f, 0.2f, 0.2f, 0f));
        GraphicApi.SetCursorMode(IGraphicApi.CursorMode.Centered);
        
        // world load
        {
            var result = ResourceManager.LoadOrCreateWorld();
            if (result.Error != null)
                return result.Error;
            _world = result.GetOrThrow();
            
            _world.AddSystem(new GraphicSyncSystem(GraphicEngine));
            EditorSystem = new EditorSystem(this);
            _world.AddSystem(EditorSystem);
        }
        
        // adding models
        Query.For(_world)
            .With<ModelComponent>()
            .Build().ForEach((ref ModelComponent modelComponent) =>
            {
                GraphicEngine.Objects[modelComponent.GraphicId] = new GraphicObject()
                {
                    Model = GraphicApi.CreateStaticRenderable(modelComponent.Model.GetOr(Models.ErrorCube.GetOrThrow())),
                    IsHidden = modelComponent.IsHidden,
                    DropsShadow = modelComponent.DropsShadow
                };
            });
        
        // adding lights
        Query.For(_world)
            .With<SpotLightComponent>()
            .Build().ForEach((ref SpotLightComponent light) =>
            {
                GraphicEngine.SpotLights[light.GraphicId] = new SpotLight();
            });
        
        // controllers
        {
            WasdController = new FlyingPlayerController(GraphicApi, InputHandler, _wasd, 0.2f);
        }

        UiManager = new UiManager(this, font);
        
        return null;
    }
    
    public enum Focus
    {
        Scene,
        ObjectInspector
    }

    public void MainCycle()
    {
        Utils.RunTargetFpsLoopWhile(
            300, 
            () => !GraphicApi.ShouldStop(),
            GraphicCycle
            );
        var error = ResourceManager.Save(_world);
        error?.Log(this);
    }

    private void GraphicCycle(float deltaTime)
    {
        InputHandler.PrepareAtFrameStart();
        _world.Update();
        
        // UI
        UiManager.Update(deltaTime); 
        
        // wasd && editor controller
        if (!UiManager.Ui.IsFocused)
        {
            WasdController.Update(deltaTime);
        }
        
        // render
        GraphicEngine.Update();
        
        // keyboard input
        if (InputHandler.IsPressed(_dumbTexturesBind)) 
            GraphicApi.DumpTextures(ExternalPath.Of("."));
        
        
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