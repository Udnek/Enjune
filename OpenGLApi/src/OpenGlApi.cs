using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Graphic;
using Enjune.Graphic.Asset;
using Enjune.Graphic.GraphicApi;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Texture;
using OpenGLApi.Data;
using OpenGLApi.Shader;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGLApi;

public sealed partial class OpenGlApi : GlDisposable, IGraphicApi
{
    private const TextureUnit MainTexturesUnit = TextureUnit.Texture0;
    private const TextureUnit ScreenTextureUnit = TextureUnit.Texture1;
    private const TextureUnit ShadowMapTextureUnit = TextureUnit.Texture2;
    private const int ShadowMapResolution = 1024;
    private const int MaxLights = 5;

    private const int MaterialsSsboBinding = 0;
    private const int MaterialIdSsboBinding = 1;
    private const int LightsSsboBinding = 2;
    
    private unsafe Window* _window;
    
    private CompiledAssets _assets = null!;
    
    // buffers
    private Vao _materialVao = null!;
    private Vbo<MaterialVertexData> _materialVbo = null!;
    private Ssbo<MatId> _matIdSsbo = null!;
    private Ssbo<PointLightData> _lightSsbo = null!;
    private Ssbo<MaterialData> _materialSsbo = null!;
    private Ebo _ebo = null!;
    private Vao _screenVao = null!;
    private Vbo<(Vector2 position, TextureCoord texCoord)> _screenVbo = null!;

    private ScreenPack _screenPack = null!;
    
    private Vao _colorVao = null!;
    private Vbo<ColoredVertexData> _colorVertexVbo = null!;

    private TextureArray _textureArray = null!;
    
    // shaders
    private MaterialShader _materialShader = null!;
    private ColorShader _colorShader = null!;
    private ScreenShader _screenShader = null!;
    
    // storing it here fucking gc won't erase it
    private GLFWCallbacks.KeyCallback _keyCallback = null!;
    private GLFWCallbacks.CursorPosCallback _cursorCallback = null!;
    private GLFWCallbacks.MouseButtonCallback _mouseButtonCallback = null!;
    private GLFWCallbacks.FramebufferSizeCallback _windowSizeChangeCallback = null!;
    private DebugProc _debugProc = null!;


    public Error? Init(CompiledAssets assets, int width, int height, string title, int verticesCapacity,
        IUserInputHandler keyHandler,
        IGraphicApi.WindowSizeChangeHandler windowSizeHandler)
    {
        _assets = assets;
        InitKeyCodeMaps();
        
        // error callback
        GLFW.SetErrorCallback((error, description) => Logger.Error(this, $"{error}: {description}"));

        if (!GLFW.Init())
            return "unable to initialize GLFW";

        // GLFW configuration
        GLFW.DefaultWindowHints();
        GLFW.WindowHint(WindowHintBool.Visible, true);
        GLFW.WindowHint(WindowHintBool.Resizable, true);
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 6);
        GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Compat);
        GLFW.WindowHint(WindowHintBool.OpenGLForwardCompat, true);

        // window creation
        unsafe
        {
            _window = GLFW.CreateWindow(width, height, title, null, null);
            if (_window == null)
                return "failed to create GLFW window";
            
            // callbacks
            
            IGraphicApi.KeyAction FromGlwf(InputAction glfw)
            {
                return glfw switch
                {
                    InputAction.Release => IGraphicApi.KeyAction.Release,
                    InputAction.Press => IGraphicApi.KeyAction.Press,
                    InputAction.Repeat => IGraphicApi.KeyAction.Repeat,
                    _ => throw new Exception($"unknown key action: {glfw}")
                };
            }
            
            _keyCallback = (window, key, scancode, action, mods) =>
            {
                if (_glfwKeyToKeyCode.TryGetValue(key, out var keyCode)) 
                    keyHandler.HandleKey(keyCode, FromGlwf(action));
            };
            GLFW.SetKeyCallback(_window, _keyCallback);
            
            _mouseButtonCallback = (window, button, action, mods) =>
            {
                if (_glfwMouseToKeyCode.TryGetValue(button, out var keyCode)) 
                    keyHandler.HandleKey(keyCode, FromGlwf(action));
            };
            GLFW.SetMouseButtonCallback(_window, _mouseButtonCallback);
            
            _windowSizeChangeCallback = (_, newWidth, newHeight) =>
            {
                ViewPort(newWidth, newHeight);
                _screenPack.Resize((newWidth, newHeight));
                windowSizeHandler(newWidth, newHeight);
            };
            GLFW.SetFramebufferSizeCallback(_window, _windowSizeChangeCallback);

            _cursorCallback = (window, x, y) => keyHandler.HandleCursor((int)x, (int)y);
            GLFW.SetCursorPosCallback(_window, _cursorCallback);
            // end callbacks

            if (GLFW.RawMouseMotionSupported())
            {
                GLFW.SetInputMode(_window, RawMouseMotionAttribute.RawMouseMotion, true);
                Logger.Log(this,"raw mouse input enabled");
            }
            else
                Logger.Warn(this,"raw mouse input isn't supported; mouse motion might be janky");
            
            GLFW.MakeContextCurrent(_window);
            GLFW.ShowWindow(_window);
        }
        
        // without that shit it won't work
        GL.LoadBindings(new GLFWBindingsContext());
        
        Logger.Log(this, $"OpenGL version: {GL.GetString(StringName.Version)}");
        
        // enable features
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.CullFace);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        // debug
        GL.Enable(EnableCap.DebugOutput);
        _debugProc = (source, type, id, severity, length, messagePointer, param) =>
        {
            string message = Marshal.PtrToStringUTF8(messagePointer, length);
            if (type == DebugType.DebugTypeError)
                Logger.Error(this, message);
            else
                Logger.Log(this, message);
        };
        GL.DebugMessageCallback(_debugProc, IntPtr.Zero);
        
        // just in case
        GL.GetInteger(GetPName.MaxTextureSize, out var maxTextureSize);
        GL.GetInteger(GetPName.MaxArrayTextureLayers, out var maxArrayLayers);
        Logger.Log(this, $"max possible texture size: {maxTextureSize}");
        Logger.Log(this, $"max possible texture array layers: {maxArrayLayers}");
        
        // other components init
        return InitComponents(verticesCapacity);
    }
    
    
    private Error? InitComponents(int verticesCapacity)
    {
        // loading comps
        _textureArray = TextureArray.FromAssets(MainTexturesUnit, _assets);
        _materialVao = new Vao();
        _materialSsbo = new Ssbo<MaterialData>(MaterialsSsboBinding, _assets.Materials.Length);
        _matIdSsbo = new Ssbo<MatId>(MaterialIdSsboBinding, verticesCapacity);
        _lightSsbo = new Ssbo<PointLightData>(LightsSsboBinding, MaxLights);
        _materialVbo = new Vbo<MaterialVertexData>(verticesCapacity);
        _ebo = new Ebo(verticesCapacity);
        _colorVao = new Vao();
        _colorVertexVbo = new Vbo<ColoredVertexData>(verticesCapacity);
        _screenVao = new Vao();
        _screenVbo = new Vbo<(Vector2 position, TextureCoord texCoord)>(6);
        _screenPack = new ScreenPack(GetWindowSize(), ScreenTextureUnit);

        
        _screenVbo.BindAndPush([
            ((-1, -1), (0, 0)), ((1, -1), (1, 0)), ((1, 1), (1, 1)), // first triangle
            ((-1, -1), (0, 0)), ((1, 1), (1, 1)), ((-1, 1), (0, 1)) // second
        ]);
        
        // loading shaders
        _materialShader = new MaterialShader(_textureArray, _lightSsbo);
        var error = _materialShader.Init(
            AssemblyPath.Of(Enjune.Enjune.Assembly, "OpenGL", "Shaders", "Material", "frag.frag"),
            AssemblyPath.Of(Enjune.Enjune.Assembly, "OpenGL", "Shaders", "Material", "vert.vert"));
        if (error != null) return error;
        
        _colorShader = new ColorShader();
        error = _colorShader.Init(
            AssemblyPath.Of(Enjune.Enjune.Assembly, "OpenGL", "Shaders", "Color", "frag.frag"),
            AssemblyPath.Of(Enjune.Enjune.Assembly, "OpenGL", "Shaders", "Color", "vert.vert"));
        if (error != null) return error;
        
        _screenShader = new ScreenShader(_screenVao, _screenVbo, _screenPack.Texture);
        error = _screenShader.Init(
            AssemblyPath.Of(Enjune.Enjune.Assembly, "OpenGL", "Shaders", "Screen", "frag.frag"),
            AssemblyPath.Of(Enjune.Enjune.Assembly, "OpenGL", "Shaders", "Screen", "vert.vert"));
        if (error != null) return error;
        new VaoAttributes(_screenVao, _screenVbo)
            .Add<float>(VertexAttribPointerType.Float, "aPos", 2)
            .Add<float>(VertexAttribPointerType.Float, "aTexPos", 2)
            .Compile(_screenShader);
        
        
        // loading materials
        {
            MaterialData ToData(CompiledMaterial mat) => new(mat.Raw.Color, mat.TextureId);
            _materialSsbo.BindAndPush(_assets.Materials.Select(ToData).ToArray());
        }
        
        // enabling off-screen rendering
        _screenPack.BindFbo();
        
        return null;
    }

    public IRenderableModel<IShader.I3D.IMaterial> CompileModel(MaterialModel model) 
        => new GlMaterialModel(_materialShader, MaterialIdSsboBinding, model);

    public IRenderableModel<IShader.I3D.IColor> CompileModel(ColorModel model) 
        => new GlColorModel(_colorShader, model);

    public void UpdateScreen()
    {
        // rendering to window
        Fbo.BindDefault();
        GL.Disable(EnableCap.DepthTest);
        _screenShader.Bind();
        _screenShader.Render();
        unsafe { GLFW.SwapBuffers(_window); }
        
        // backing off-screen rendering
        GL.Enable(EnableCap.DepthTest);
        ShaderProgram.Unbind();
        _screenPack.BindFbo();
    }
    
    public void UseShader<T>(Consumer<T> consumer) where T : IShader
    {
        ShaderProgram shader;
        if (typeof(T) == typeof(IShader.I3D.IMaterial))
            shader = _materialShader;
        else if (typeof(T) == typeof(IShader.I3D.IColor))
            shader = _colorShader;
        else
        {
            Logger.Error(this, $"shader isn't supported: {typeof(T)}");
            return;
        }
        shader.Bind();
        consumer((T)(object)shader); // todo probably fuck around it?
        ShaderProgram.Unbind();
    }
    
    public void ClearScreenBuffers(bool color = true, bool depth = true)
    {
        GL.Clear(
            (color ? ClearBufferMask.ColorBufferBit : 0) | 
            (depth ? ClearBufferMask.DepthBufferBit : 0));
    }

    protected override void DisposeGlData()
    {
        Utils.DisposeAllFields(this);
        
        unsafe
        {
            GLFW.SetKeyCallback(_window, null);
            GLFW.SetFramebufferSizeCallback(_window, null);
            GLFW.DestroyWindow(_window);
        }

        GLFW.Terminate();
        GLFW.SetErrorCallback(null);
    }
}