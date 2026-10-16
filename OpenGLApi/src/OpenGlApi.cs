using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Texture;
using OpenGLApi.Mesh;
using OpenGLApi.Pack;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGLApi;

public sealed partial class OpenGlApi : GlDisposable, IGraphicApi, IRawGraphicApi
{
    private const TextureUnit MainTexturesUnit = TextureUnit.Texture0;
    
    private unsafe Window* _window;
    
    private CompiledAssets _assets = null!;
    
    
    // storing it here so fucking GC won't erase it
    private GLFWCallbacks.KeyCallback _keyCallback = null!;
    private GLFWCallbacks.CharCallback _charCallback = null!;
    private GLFWCallbacks.CursorPosCallback _cursorCallback = null!;
    private GLFWCallbacks.MouseButtonCallback _mouseButtonCallback = null!;
    private GLFWCallbacks.FramebufferSizeCallback _windowSizeChangeCallback = null!;
    private DebugProc _debugProc = null!;
    private GLFWCallbacks.ScrollCallback _scrollCallback = null!;
    
    // for shader generation
    private readonly Dictionary<Type, BufferDesc> _ssboDescs = [];
    private readonly Dictionary<Type, BufferDesc> _uboDescs = [];
    
    // disposables
    private readonly List<IGlBuffer> _bufferToDispose = [];
    private readonly List<IGlMesh> _meshesToDispose = [];
    private readonly List<IGlShader> _shadersToDispose = [];

    public IGraphicApi? Init(CompiledAssets assets, Vector2i windowSize, string title, IUserInputHandler inputHandler,
        out Error? error)
    {
        var graphicApi = InitInternal(assets, windowSize, title, inputHandler, out error);
        if (graphicApi != null) return graphicApi;
        Dispose();
        return null;
    }

    private OpenGlApi? InitInternal(CompiledAssets assets, Vector2i initialWindowSize, string title, IUserInputHandler inputHandler,
        out Error? error)
    {
        _assets = assets;
        InitKeyCodeMaps();
        
        // error callback
        GLFW.SetErrorCallback((error, description) => Logger.Error(this, $"{error}: {description}"));

        if (GLFW.PlatformSupported(Platform.X11))
        {
            Logger.Info(this, "X11 is supported, switching to it");
            GLFW.InitHint(InitHintPlatform.Platform, Platform.X11);
        }
        
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
        
        GLFW.WindowHint(WindowHintBool.TransparentFramebuffer, false);
        
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
            GLFW.MakeContextCurrent(_window);
            
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

            _charCallback = (window, codepoint) => inputHandler.HandleCharacter((char) codepoint);
            GLFW.SetCharCallback(_window, _charCallback);
            
            _mouseButtonCallback = (window, button, action, mods) =>
            {
                if (_glfwMouseToKeyCode.TryGetValue(button, out var keyCode)) 
                    inputHandler.HandleKey(keyCode, FromGlwf(action));
            };
            GLFW.SetMouseButtonCallback(_window, _mouseButtonCallback);
            
            _windowSizeChangeCallback = (_, w, h) => inputHandler.HandleWindowSizeChange((w, h));
            GLFW.SetFramebufferSizeCallback(_window, _windowSizeChangeCallback);

            _cursorCallback = (window, x, y) =>
            {
                inputHandler.HandleCursorFromLeftBottom((int)x, GetWindowSize().Y - (int)y);
            };
            GLFW.SetCursorPosCallback(_window, _cursorCallback);

            _scrollCallback = (window, x, y) => inputHandler.HandleScroll((float) x, (float) y);
            GLFW.SetScrollCallback(_window, _scrollCallback);
            
            // end callbacks

            if (GLFW.RawMouseMotionSupported())
            {
                GLFW.SetInputMode(_window, RawMouseMotionAttribute.RawMouseMotion, true);
                Logger.Info(this,"raw mouse input enabled");
            }
            else
                Logger.Warn(this,"raw mouse input isn't supported; mouse motion might be janky");
            
            GLFW.ShowWindow(_window);
        }
        
        // without that shit it won't work
        GL.LoadBindings(new GLFWBindingsContext());
        
        Logger.Info(this, $"Platform: {GLFW.GetPlatform()}");
        Logger.Info(this, $"OpenGL version: {GL.GetString(StringName.Version)}");
        
        // enable features
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        // culling
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);
        
        // red pixel texture fix when 2x2
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        
        // debug
        GL.Enable(EnableCap.DebugOutput);
        _debugProc = (source, type, id, severity, length, messagePointer, param) =>
        {
            string message = Marshal.PtrToStringUTF8(messagePointer, length);
            if (type == DebugType.DebugTypeError)
                Logger.Error(this, message);
            else
                Logger.Info(this, message);
        };
        GL.DebugMessageCallback(_debugProc, IntPtr.Zero);
        
        // just in case
        GL.GetInteger(GetPName.MaxTextureSize, out var maxTextureSize);
        GL.GetInteger(GetPName.MaxArrayTextureLayers, out var maxArrayLayers);
        Logger.Info(this, $"max possible texture size: {maxTextureSize}");
        Logger.Info(this, $"max possible texture array layers: {maxArrayLayers}");
        
        // other components init
        error = InitComponents();
        if (error != null) return null;
        return this;
    }
    
    private Error? InitComponents()
    {
        // loading comps
        //_textureArray = TextureArray.FromAssets(MainTexturesUnit, _assets);
        return null;
    }

    public ResultOrError<IShader<TPerVert, TPerPrim>> CreateShader<TPerVert, TPerPrim, TVaryings>(string userVertexCode, string userFragmentCode) where TPerVert : unmanaged where TPerPrim : unmanaged where TVaryings : unmanaged
    {
        var vaoVboDeviceOrErr = VaoVboDevice.Get<TPerVert>();
        if (vaoVboDeviceOrErr.Error is not null)
            return vaoVboDeviceOrErr.Error.Value;
        
        var ssboDescOrErr = BufferDesc.Get<TPerPrim>(Bindings.PerPrimitiveSsbo, Std.Std430, "PerPrimitives", "PerPrimitive");
        if (ssboDescOrErr.Error is not null)
            return ssboDescOrErr.Error.Value;

        _ssboDescs[typeof(TPerPrim)] = ssboDescOrErr.GetOrThrow();

        var varyingsOrErr = StructDesc.Get<TVaryings>(null);
        if (varyingsOrErr.Error is not null)
            return varyingsOrErr.Error.Value;

        var vert = ShaderGenerator.CreateVertex(
            userVertexCode,
            vaoVboDeviceOrErr.GetOrThrow().VaoAttributes,
            _ssboDescs.Values.ToArray(),
            _uboDescs.Values.ToArray(),
            varyingsOrErr.GetOrThrow()
        );
        
        var frag = ShaderGenerator.CreateFragment(
            userFragmentCode,
            _ssboDescs.Values.ToArray(),
            _uboDescs.Values.ToArray(),
            varyingsOrErr.GetOrThrow()
        );
        
        // Logger.Highlight(this, "\n"+vert);
        // Logger.Highlight(this, "-----------------");
        // Logger.Highlight(this, "\n"+frag);

        var shader = new ShaderProgram<TPerVert, TPerPrim>();
        _shadersToDispose.Add(shader);
        var shaderError = shader.Init(vert, frag);
        if (shaderError != null)
            return shaderError.Value;
        
        return ResultOrError.Success<IShader<TPerVert, TPerPrim>>(shader);
    }

    public ResultOrError<IUniform<T>> CreateUniform<T>(string name) where T : unmanaged
    {
        var descOrErr = BufferDesc.Get<T>(Bindings.TakeUbo(name), Std.Std140, name, null);
        if (descOrErr.Error != null) 
            return descOrErr.Error.Value;
        var desc = descOrErr.GetOrThrow();
        _uboDescs[typeof(T)] = desc;
        var ubo = new Ubo<T>(Bindings.TakeUbo(name));
        _bufferToDispose.Add(ubo);
        return ResultOrError.Success<IUniform<T>>(ubo);
    }

    public ResultOrError<IUniformArray<T>> CreateUniformArray<T>(string name, int initialCapacity) where T : unmanaged
    {
        var descOrErr = BufferDesc.Get<T>(Bindings.TakeSsbo(name), Std.Std430, name, null);
        if (descOrErr.Error != null) 
            return descOrErr.Error.Value;
        var desc = descOrErr.GetOrThrow();
        _ssboDescs[typeof(T)] = desc;
        var ssbo = new Ssbo<T>(Bindings.TakeSsbo(name), initialCapacity, false);
        _bufferToDispose.Add(ssbo);
        return ResultOrError.Success<IUniformArray<T>>(ssbo);
    }

    public void SetRenderSize(Vector2i size)
    {
        Fbo.SizeOfDefault = size;
        //_screenPack.Resize(size);
    }

    public void UpdateScreen()
    {
        // rendering to window
        unsafe { GLFW.SwapBuffers(_window); }
    }
    
    public void ClearRenderBuffer(bool color = true, bool depth = true)
    {
        GL.Clear(
            (color ? ClearBufferMask.ColorBufferBit : 0) | 
            (depth ? ClearBufferMask.DepthBufferBit : 0));
    }

    protected override void DisposeGlData()
    {
        foreach (var glMesh in _meshesToDispose) glMesh.Dispose();
        foreach (var glShader in _shadersToDispose) glShader.Dispose();
        foreach (var glBuffer in _bufferToDispose) glBuffer.Dispose();
        
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