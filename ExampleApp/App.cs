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
using ExampleApp.Data;
using OpenGLApi;
using OpenTK.Mathematics;

namespace ExampleApp;

public class App : AbstractDisposable, IApp 
{
    private static readonly Vector2i InitialWindowSize = (480*2, 360*2);
    private const string Title = "Example";

    private IGraphicApi GraphicApi { get; set; } = null!;
    private readonly BasicInputHandler _inputHandler;
    private FlyingPlayerController FlyingController { get; set; } = null!;
    private readonly KeyBinds _binds;
    private readonly Wasd _wasd;
    private IUniform<CameraUniform> CameraUniform { get; set; }
    private IUniform<TimeUniform> TimeUniform { get; set; }
    private IShader<PerVertex, PerPrimitive> MainShader { get; set; }
    private bool _isRunning = true;
    private IDynamicTexture _offScreenTex;
    private ITextureSlot<ITexture> _slot;
    private ITexture _mainTex;
    private IShader<PerVertex, PerPrimitive> ScreenShader { get; set; }

    public App()
    {
        _binds = KeyBinds.CreateEmpty();
        _wasd = Wasd.AddTo(_binds);
        
        _inputHandler = new BasicInputHandler(InitialWindowSize, 0.5f, 
            () => _isRunning = false);
    }

    public Error? Init()
    {
        var image = AssemblyPath.Of(this.GetAssembly(), "Atlas.png").LoadImage(out var imgErr);
        if (image is null) return imgErr;

        // graphicApi
        {
            var graphicApi = new OpenGlApi().Init(
                InitialWindowSize,
                $"{Title} ({InitialWindowSize.X}x{InitialWindowSize.Y})",
                _inputHandler,
                out var graphicError);
            
            if (graphicApi == null) 
                return graphicError;
            GraphicApi = graphicApi;
            
            GraphicApi.SetVsync(false);
            GraphicApi.SetClearColor(new Vector4(50, 50, 50, 255f)/255f);
            GraphicApi.SetCursorMode(IGraphicApi.CursorMode.Centered);

            if (GraphicApi.CreateUniform<CameraUniform>("camera").GetIsFailed(out var cu, out var error))
                return "cant create uniform: " + error;
            CameraUniform = cu;

            if (GraphicApi.CreateUniform<TimeUniform>("time").GetIsFailed(out var tu, out error))
                return "cant create uniform: " + error;
            TimeUniform = tu;
            
            if (GraphicApi.CreateTexture(image).GetIsFailed(out _mainTex!, out error))
                return "cant create texture: " + error;

            if (GraphicApi.CreateTextureSlot<ITexture>("mainTex").GetIsFailed(out _slot!, out error))
                return "cant create slot: " + error;
            
            _slot.BindToSample(_mainTex);
            
            if (GraphicApi.CreateDynamicTexture((300, 300)).GetIsFailed(out _offScreenTex!, out error))
                return "cant create texture: " + error;

            if (GraphicApi.CreateShader<PerVertex, PerPrimitive, Varyings>(
                    """
                    vec4 process(Input input, out Varyings varyings) {
                        varyings.color = input.color;
                        varyings.uv = input.uv;
                        return camera.proj * camera.view * vec4(input.pos, 1);
                    }
                    """,
                    """
                    vec4 process(Varyings varyings, PerPrimitive perPrim) {
                        return varyings.color * 
                                perPrim.color * 
                                texture(mainTex, varyings.uv) * 
                                (sin(time.sec)+1)/2.0;
                    }
                    """
                ).GetIsFailed(out var shader, out error))
            {
                return "cant create shader: " + error;
            }

            MainShader = shader;
            
            if (GraphicApi.CreateShader<PerVertex, PerPrimitive, Varyings>(
                    """
                    vec4 process(Input input, out Varyings varyings) {
                        varyings.color = input.color;
                        varyings.uv = input.uv;
                        
                        vec3 pos = input.pos;
                        pos.y += sin(time.sec*1.5) / 10.0;
                        return vec4(pos, 1);
                    }
                    """,
                    """
                    vec4 process(Varyings varyings, PerPrimitive perPrim) {
                        return texture(mainTex, varyings.uv);
                    }
                    """
                ).GetIsFailed(out shader, out error))
            {
                return "cant create shader: " + error;
            }

            ScreenShader = shader;
        }
        
        // controllers
        FlyingController = new FlyingPlayerController(GraphicApi, _inputHandler, _wasd)
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
            new PerPrimitive() { Color = new Vector4(1, 1, 0, 1f) },
            (0, 0, 0.5f), (10, 10, 0.5f), (0, 10, 0.5f), TextureQuad.Full);
        mesh.AddNgon(
            new PerPrimitive() { Color = (0f, 1, 1, 1) },
            new PerVertex() { Position = (0, 10, 0.5f), Color = (1, 0, 0, 1), Uv = (0, 0) },
            new PerVertex() { Position = (10, 10, 0.5f), Color = (0, 1, 0, 1), Uv = (1, 0) },
            new PerVertex() { Position = (5, 15, 0.5f), Color = (0, 0, 1, 1), Uv = (0.5f, 0.5f) }
        );
        mesh.Scale(new Vector3(0.5f));

        if (GraphicApi.CreateRenderableMesh(mesh).GetIsFailed(out var renderableHouseMesh, out var error))
        {
            Logger.Error(this, error);
            return;
        }

        mesh.Clear();
        mesh.AddQuad(new PerVertex(), new PerPrimitive(),
            (-1, -1, 0), (1, -1, 0), (1, 1, 0), (-1, 1, 0),
            TextureQuad.Full);
        
        if (GraphicApi.CreateRenderableMesh(mesh).GetIsFailed(out var screenMesh, out error))
        {
            Logger.Error(this, error);
            return;
        }
        
        var stopwatch = Stopwatch.StartNew();

        Utils.RunTargetFpsLoopWhile(
            200, 
            () => _isRunning,
            dt =>
            {
                _inputHandler.PrepareAtFrameStart();
                
                // wasd
                FlyingController.Update(dt);
        
                // window
                if (_inputHandler.WindowSizeChanged) 
                    GraphicApi.SetTitle($"{Title} ({_inputHandler.WindowSize.X}x{_inputHandler.WindowSize.Y})");
                
                // updating uniforms
                CameraUniform.Set(new CameraUniform()
                {
                    Projection = Matrix4.CreatePerspectiveFieldOfView(
                        MathF.PI / 2, (float) _inputHandler.WindowSize.X / _inputHandler.WindowSize.Y, 0.1f, 1000f),
                    View = FlyingController.View
                });
                TimeUniform.Set(new TimeUniform(){Seconds = (float) stopwatch.Elapsed.TotalSeconds});
        
                // render
                _slot.BindToSample(_mainTex);
                _offScreenTex.BindAsRenderTarget();
                GraphicApi.SetClearColor((0.4f, 0.4f, 1f, 1));
                GraphicApi.ClearCurrentRenderTarget();
                MainShader.Render(renderableHouseMesh);
                // then render to screen
                _slot.BindToSample(_offScreenTex);
                GraphicApi.BindScreenRenderTarget();
                GraphicApi.SetClearColor(new Vector4(50, 50, 50, 255f)/255f);
                GraphicApi.ClearCurrentRenderTarget();
                ScreenShader.Render(screenMesh);
        
                // post frame
                GraphicApi.UpdateScreen();
                _inputHandler.ClearForNextFrame();
                GraphicApi.UpdateEvents(); 
            });
    }

    protected override void DisposeData()
    {
        Utils.DisposeAllFields(this);
    }
}