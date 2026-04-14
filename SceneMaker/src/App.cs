using Enjune;
using Enjune.File;
using Enjune.File.ModelReader;
using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.Font;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.GraphicApi.OpenGL;
using Enjune.Graphic.InputHandler;
using Enjune.Misc;
using Enjune.World;
using OpenTK.Mathematics;

namespace SceneMaker;

public class App : IApp
{
    private int _windowWidth = 480*2;
    private int _windowHeight = 360*2;
    
    private readonly IGraphicApi _grapi = new OpenGlApi();
    private readonly KeyBinds _binds;
    private readonly Wasd _wasd;
    private readonly KeyBinds.Bind _dumbTexturesBind;
    
    private readonly BasicInputHandler _inputHandler;
    private FlyingPlayerController _wasdController = null!;
    
    private readonly VertexBuffer _vertexBuffer = new VertexBuffer(20_000);
    //private readonly List<Model> _models = new();

    private IGraphicApi.DrawMode _drawMode = IGraphicApi.DrawMode.Fill;
    private readonly KeyBinds.Bind _freeCursorBind;
    private Scene _scene;
    private readonly EditorController _editorController;

    public App()
    {
        _binds = KeyBinds.CreateEmpty();
        KeyBinds.AddWasd(_binds, out _wasd);
        _freeCursorBind = new KeyBinds.Bind("free_cursor", GlfwKey.Escape);
        _binds.AddBind(_freeCursorBind);
        _dumbTexturesBind = _binds.AddBind(new KeyBinds.Bind("dumb_textures", GlfwKey.F2));
        
        _scene = new Scene();
        
        _inputHandler = new BasicInputHandler(_grapi, _binds);
        _wasdController = new FlyingPlayerController(_grapi, _inputHandler, _wasd, 0.2f);
        _editorController = new EditorController(_grapi, _inputHandler, _scene);
    }
    
    private void WindowSizeChangeHandler(int w, int h)
    {
        _windowWidth = w;
        _windowHeight = h;
        _grapi.ViewPort(0, 0, w, h);
    }
    
    public Error? Init()
    {
        var assetManager = new AssetManager();

        var watchTower = new DotObjModelReader(assetManager, AssemblyPath.Of(Enjune.Enjune.Assembly,"Models", "wt", "wooden watch tower2.obj"))
            .Read(out var error);
        if (watchTower == null) return error;

        watchTower.Meshes[0].Item2.Raw.Color = (1, 1, 1, 1);
        Logger.Log(this, $"{nameof(watchTower)} info: {watchTower.Info()}");

        var font = assetManager.AddFont(AssemblyPath.Of(Enjune.Enjune.Assembly, "Fonts", "papyrus.ttf"), 128, out error);
        if (font == null) return error;

        var assets = assetManager.Compile();

        error = _grapi.Init(assets, _windowWidth, _windowHeight, "Enjune C#", _inputHandler, WindowSizeChangeHandler);
        if (error != null) return error;
        _grapi.SetClearColor(new Color(0.2f, 0.2f, 0.2f, 0f));
        
        _grapi.SetCursorMode(IGraphicApi.CursorMode.Centered);
        
        _scene.Objects.Add(new SObject(watchTower));
        _scene.Objects.Add(new SObject(font.Generate("Niggers", 10f), true));
        
        // _vertexBuffer.Clear();
        // foreach (var m in _models)
        // {
        //     _vertexBuffer.PutModel(m);
        // }
        return null;
    }

    public void Run()
    {
        var pixelBuffer = new FixedBuffer<Vector2>(9999999);

        var delays = new List<long>(200);
        int tick = 0;
        float deltaTime = 0;
        Utils.RunTargetFpsLoopWhile(100,
            out deltaTime,
            delay =>
            {
                delays.Add(delay);
            },
            () => !_grapi.ShouldStop(),
            () =>
            {
                _wasdController.Update(deltaTime);
                
                var projection = Matrix4.CreatePerspectiveFieldOfView(
                    MathF.PI / 2, (float) _windowWidth / _windowHeight, 0.1f, 1000f);
                _grapi.ProjectionTransform(projection);

                var view = _wasdController.View;
                _grapi.ViewTransform(view);
                
                _editorController.Update(view, projection);
                
                // other inputs
                if (_inputHandler.IsPressed(_freeCursorBind))
                    _grapi.SetCursorMode(IGraphicApi.CursorMode.Normal);
                
                if (_inputHandler.IsPressed(_dumbTexturesBind)) 
                    _grapi.DumpTextures(ExternalPath.Of("."));
                
                // render
                
                _grapi.ClearScreenBuffers();
                
                foreach (var sObject in _scene.Objects)
                {
                    // sObject.Position.X += 1f*deltaTime;
                    // sObject.Rotation *= Quaternion.FromAxisAngle(Vector3.UnitZ, 1*deltaTime);
                    sObject.Scale.Y = 2;
                    _vertexBuffer.Clear();
                    _vertexBuffer.PutModel(sObject.Model);
                     _grapi.ModelTransform(sObject.ModelMatrix);
                    // _grapi.SetDrawMode(IGraphicApi.DrawMode.Fill);
                    // _grapi.RenderToScreenBuffer(_vertexBuffer);

                    
                    if (sObject.IsText)
                    {
                        //_grapi.SwitchShader(IGraphicApi.ShaderType.Text);
                        _grapi.GlobalColor(new Color(MathF.Sin(tick/60f)/2 + 0.5f, MathF.Cos(tick/30f)/2 + 0.5f, 0, 0.6f));
                    }
                    else
                    {
                        //_grapi.SwitchShader(IGraphicApi.ShaderType.Main);
                        _grapi.GlobalColor(new Color(1));
                    }
                    
                    if (sObject == _editorController.SelectedObject)
                    {
                        _grapi.GlobalColor((1, 0.5f, 0f, 1f));
                    }
                    else
                    {
                        _grapi.GlobalColor(new Color(1f));
                    }
                    
                    _grapi.RenderToScreenBuffer(_vertexBuffer);
                }
                
                _grapi.RenderPixelsToScreenBuffer(pixelBuffer);
                
                // end
                _inputHandler.ClearForNextFrame();
                _grapi.UpdateScreen();
                _grapi.UpdateEvents();
                tick += 1;
            });
        
        var avgDelay = delays.Sum(v => v) / delays.Count;
        Console.WriteLine($"Avg delay: {avgDelay}; avg possible fps: {Utils.NanoDelayToFps(avgDelay)}");
        
    }

    public void Dispose()
    {
        _grapi.Dispose();
    }
}