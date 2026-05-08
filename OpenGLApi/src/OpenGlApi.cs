using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Texture;
using OpenGLApi.Data;
using OpenGLApi.Model;
using OpenGLApi.Pack;
using OpenGLApi.Shader;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGLApi;

public sealed partial class OpenGlApi : GlDisposable, IGraphicApi, IRawGraphicApi
{
    private const TextureUnit MainTexturesUnit = TextureUnit.Texture0;
    private const TextureUnit ScreenTextureUnit = TextureUnit.Texture1;
    private const TextureUnit ShadowMapTextureUnit = TextureUnit.Texture2;
    private const int ShadowMapResolution = 2048;
    private const int MaxLights = 5; // MUST BE ALSO CHANGED INSIDE VERTEX AND FRAG SHADERS

    private const int MaterialsSsboBinding = 0;
    private const int PerPrimitiveSsboBinding = 1;
    private const int LightsSsboBinding = 2;
    
    private unsafe Window* _window;
    
    private CompiledAssets _assets = null!;
    
    // buffers
    private SsboDataAndArray<LightsLengthData, SpotLightData> _lightSsbo = null!;
    private SsboArray<MaterialData> _materialSsbo = null!;
    private Vao _screenVao = null!;
    private Vbo<(Vector2 position, Vector2 texCoord)> _screenVbo = null!;

    private ScreenPack _screenPack = null!;
    private ShadowMapPack _shadowMapPack = null!;
    

    private TextureArray _textureArray = null!;
    
    // shaders
    private MaterialShader _materialShader = null!;
    private ColorShader _colorShader = null!;
    private ScreenShader _screenShader = null!;
    private ShadowMapShader _shadowMapShader = null!;

    private readonly Dictionary<Type, AbstractShader> _typeToShader = [];
    
    // storing it here fucking gc won't erase it
    private GLFWCallbacks.KeyCallback _keyCallback = null!;
    private GLFWCallbacks.CursorPosCallback _cursorCallback = null!;
    private GLFWCallbacks.MouseButtonCallback _mouseButtonCallback = null!;
    private GLFWCallbacks.FramebufferSizeCallback _windowSizeChangeCallback = null!;
    private DebugProc _debugProc = null!;
    private GLFWCallbacks.ScrollCallback _scrollCallback = null!;

    public IGraphicApi? Init(CompiledAssets assets, Vector2i windowSize, string title, IUserInputHandler inputHandler,
        out Error? error)
    {
        var graphicApi = InitInternal(assets, windowSize, title, inputHandler, out error);
        if (graphicApi != null) return graphicApi;
        Dispose();
        return null;
    }

    private IGraphicApi? InitInternal(CompiledAssets assets, Vector2i initialWindowSize, string title, IUserInputHandler inputHandler,
        out Error? error)
    {
        _assets = assets;
        InitKeyCodeMaps();
        
        // error callback
        GLFW.SetErrorCallback((error, description) => Logger.Error(this, $"{error}: {description}"));

        if (!GLFW.Init())
        {
            error = "unable to initialize GLFW";
            return null;
        }

        // GLFW configuration
        GLFW.DefaultWindowHints();
        GLFW.WindowHint(WindowHintBool.Visible, true);
        GLFW.WindowHint(WindowHintBool.Resizable, true);
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 4);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 6);
        GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Compat);
        GLFW.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
        
        GLFW.WindowHint(WindowHintBool.TransparentFramebuffer, true);
        
        
        // window creation
        unsafe
        {
            _window = GLFW.CreateWindow(initialWindowSize.X, initialWindowSize.Y, title, null, null);
            
            Fbo.SizeOfDefault = initialWindowSize;
            if (_window == null)
            {
                error = "failed to create GLFW window";
                return null;
            }
            
            GLFW.SetWindowOpacity(_window, 0.5f);
            
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
                    inputHandler.HandleKey(keyCode, FromGlwf(action));
            };
            GLFW.SetKeyCallback(_window, _keyCallback);
            
            _mouseButtonCallback = (window, button, action, mods) =>
            {
                if (_glfwMouseToKeyCode.TryGetValue(button, out var keyCode)) 
                    inputHandler.HandleKey(keyCode, FromGlwf(action));
            };
            GLFW.SetMouseButtonCallback(_window, _mouseButtonCallback);
            
            _windowSizeChangeCallback = (_, newWidth, newHeight) 
                => inputHandler.HandleWindowSizeChange((newWidth, newHeight));
            GLFW.SetFramebufferSizeCallback(_window, _windowSizeChangeCallback);

            _cursorCallback = (window, x, y) => inputHandler.HandleCursor((int)x, (int)y);
            GLFW.SetCursorPosCallback(_window, _cursorCallback);

            _scrollCallback = (window, x, y) => inputHandler.HandleScroll((float) x, (float) y);
            GLFW.SetScrollCallback(_window, _scrollCallback);
            
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
        error = InitComponents();
        if (error != null) return null;
        return this;
    }
    
    private Error? InitComponents()
    {
        // loading comps
        _textureArray = TextureArray.FromAssets(MainTexturesUnit, _assets);
        _lightSsbo = new SsboDataAndArray<LightsLengthData, SpotLightData>(LightsSsboBinding, MaxLights, true);
        {
            // loading materials
            _materialSsbo = new SsboArray<MaterialData>(MaterialsSsboBinding, _assets.Materials.Length, true);
            MaterialData ToData(CompiledMaterial mat) => new(mat.Raw.Color, mat.TextureId);
            _materialSsbo.BindAndPush(_assets.Materials.Select(ToData).ToArray());
        }
        _screenVao = new Vao();
        _screenVbo = new Vbo<(Vector2 position, Vector2 texCoord)>(6, true);
        _screenPack = new ScreenPack(GetWindowSize(), ScreenTextureUnit);
        _shadowMapPack = new ShadowMapPack(ShadowMapResolution, ShadowMapTextureUnit, MaxLights);
        
        _screenVbo.BindAndPush([
            ((-1, -1), (0, 0)), ((1, -1), (1, 0)), ((1, 1), (1, 1)), // first triangle
            ((-1, -1), (0, 0)), ((1, 1), (1, 1)), ((-1, 1), (0, 1)) // second
        ]);
        
        // loading shaders
        _materialShader = new MaterialShader(_screenPack.Fbo, _textureArray, _shadowMapPack.Maps);
        var error = _materialShader.Init(
            AssemblyPath.Of(GetType().Assembly, "Shaders", "Material", "frag.frag"),
            AssemblyPath.Of(GetType().Assembly, "Shaders", "Material", "vert.vert"),
            AssemblyPath.Of(GetType().Assembly, "Shaders", "Common.glsl"));
        if (error != null) return $"{nameof(_materialShader)}: {error}";
        
        _colorShader = new ColorShader(_screenPack.Fbo, _textureArray);
        error = _colorShader.Init(
            AssemblyPath.Of(GetType().Assembly, "Shaders", "Color", "frag.frag"),
            AssemblyPath.Of(GetType().Assembly, "Shaders", "Color", "vert.vert"),
            AssemblyPath.Of(GetType().Assembly, "Shaders", "Common.glsl"));
        if (error != null) return $"{nameof(_colorShader)}: {error}";
        
        _screenShader = new ScreenShader(_screenVao, _screenVbo, _screenPack.Texture);
        error = _screenShader.Init(
            AssemblyPath.Of(GetType().Assembly, "Shaders", "Screen", "frag.frag"),
            AssemblyPath.Of(GetType().Assembly, "Shaders", "Screen", "vert.vert"),
            AssemblyPath.Of(GetType().Assembly, "Shaders", "Common.glsl"));
        if (error != null) return $"{nameof(_screenShader)}: {error}";
        new VaoAttributes(_screenVao, _screenVbo)
            .Add<float>(VertexAttribPointerType.Float, "aPos", 2)
            .Add<float>(VertexAttribPointerType.Float, "aTexPos", 2)
            .Compile(_screenShader);

        _shadowMapShader = new ShadowMapShader(_lightSsbo, _shadowMapPack);
        error = _shadowMapShader.Init(
            AssemblyPath.Of(GetType().Assembly, "Shaders", "ShadowMap", "frag.frag"),
            AssemblyPath.Of(GetType().Assembly, "Shaders", "ShadowMap", "vert.vert"),
            AssemblyPath.Of(GetType().Assembly, "Shaders", "Common.glsl"));
        if (error != null) return $"{nameof(_shadowMapShader)}: {error}";
        
        _typeToShader[typeof(IShader.ICamera.IMaterial)] = _materialShader;
        _typeToShader[typeof(IShader.ICamera.IColor)] = _colorShader;
        _typeToShader[typeof(IShader.IShadowMap)] = _shadowMapShader;
        
        
        // enabling off-screen rendering
        _screenPack.BindFbo();
        
        return null;
    }

    public void SetLights(IEnumerable<SpotLight> lights)
    {
        int count = lights.Count();
        if (count > MaxLights)
        {
            Logger.Warn(this,$"lights size to big: {count}, but max capacity is {MaxLights}");
            count = MaxLights;
        }
        _lightSsbo.BindAndPush(new LightsLengthData(count), lights.Select(l => new SpotLightData(l.View, l.Projection, l.Color, l.Position)).ToArray());
    }

    public void SetRenderSize(Vector2i size)
    {
        Fbo.SizeOfDefault = size;
        _screenPack.Resize(size);
    }

    public void UpdateScreen()
    {
        // rendering to window
        _screenShader.Bind();
        _screenShader.AfterBind();
        _screenShader.Render();
        unsafe { GLFW.SwapBuffers(_window); }
        _screenShader.BeforeUnbind();
        ShaderProgram.Unbind();
    }
    
    
    public void UseShader<T>(Consumer<T> consumer) where T : IShader
    {
        if (_typeToShader.TryGetValue(typeof(T), out var shader))
        {
            shader.Bind();
            shader.AfterBind();
            consumer((T)(object)shader); // todo probably fuck around it?
            shader.BeforeUnbind();
            ShaderProgram.Unbind();
        }
        else
            Logger.Error(this, $"shader isn't supported: {typeof(T)}");
    }
    
    public void ClearRenderBuffer(bool color = true, bool depth = true)
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