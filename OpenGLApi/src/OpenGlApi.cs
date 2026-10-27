using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Graphic;
using Enjune.Graphic.Api;
using Enjune.Misc;
using OpenGLApi.Component;
using OpenGLApi.Component.Buffer;
using OpenGLApi.Component.Texture;
using OpenGLApi.Generation;
using OpenGLApi.Mesh;
using OpenGLApi.Pack;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenGLApi;

public sealed partial class OpenGlApi : GlDisposable, IGraphicApi, IRawGraphicApi
{
    private unsafe Window* _window;
    
    // storing it here so fucking GC won't erase it
    private GLFWCallbacks.KeyCallback _keyCallback = null!;
    private GLFWCallbacks.CharCallback _charCallback = null!;
    private GLFWCallbacks.CursorPosCallback _cursorCallback = null!;
    private GLFWCallbacks.MouseButtonCallback _mouseButtonCallback = null!;
    private GLFWCallbacks.FramebufferSizeCallback _windowSizeChangeCallback = null!;
    private DebugProc _debugProc = null!;
    private GLFWCallbacks.ScrollCallback _scrollCallback = null!;
    private GLFWCallbacks.WindowCloseCallback _windowCloseCallback = null!;
    
    // for shader generation
    private readonly Dictionary<Type, BufferDesc> _perPrimDescs = [];
    private readonly Dictionary<Type, BufferDesc> _ssboDescs = [];
    private readonly Dictionary<Type, BufferDesc> _uboDescs = [];
    private readonly List<IGlTextureSlot> _textureSlots = [];
    
    // disposables
    private readonly List<IGlBuffer> _bufferToDispose = [];
    private readonly List<IGlMesh> _meshesToDispose = [];
    private readonly List<IGlShader> _shadersToDispose = [];
    private readonly List<AbstractTexture> _texturesToDispose = [];
    private readonly List<FboRboTexDevice> _rboFboToDispose = [];

    public IGraphicApi? Init(Vector2i windowSize, string title, IUserInputHandler inputHandler,
        out Error? error)
    {
        var graphicApi = InitInternal(windowSize, title, inputHandler, out error);
        if (graphicApi != null) return graphicApi;
        Dispose();
        return null;
    }

    private OpenGlApi? InitInternal(Vector2i initialWindowSize, string title, IUserInputHandler inputHandler,
        out Error? error)
    {
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
            
            FboRboTexDevice.ScreenSize = initialWindowSize;
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
                    _ => throw new Exception($"Unknown key action: {glfw}")
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
            
            _windowSizeChangeCallback = (_, w, h) =>
            {
                FboRboTexDevice.ScreenSize = (w, h);
                inputHandler.HandleWindowSizeChange((w, h));
            };
            GLFW.SetFramebufferSizeCallback(_window, _windowSizeChangeCallback);

            _cursorCallback = (window, x, y) =>
            {
                inputHandler.HandleCursorFromLeftBottom((int)x, GetWindowSize().Y - (int)y);
            };
            GLFW.SetCursorPosCallback(_window, _cursorCallback);

            _scrollCallback = (window, x, y) => inputHandler.HandleScroll((float) x, (float) y);
            GLFW.SetScrollCallback(_window, _scrollCallback);

            _windowCloseCallback = window =>
            {
                GLFW.SetWindowShouldClose(_window, false);
                inputHandler.HandleCloseWindowAttempt();
            };
            GLFW.SetWindowCloseCallback(_window, _windowCloseCallback);
            
            // end callbacks

            if (GLFW.RawMouseMotionSupported())
            {
                GLFW.SetInputMode(_window, RawMouseMotionAttribute.RawMouseMotion, true);
                Logger.Info(this,"Raw mouse input enabled");
            }
            else
                Logger.Warn(this,"Raw mouse input isn't supported: mouse motion might be janky");
            
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
            if (severity == DebugSeverity.DebugSeverityNotification) 
                return;
            
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

        error = null;
        return this;
    }
    


    public void BindScreenRenderTarget() => FboRboTexDevice.BindScreen();

    public void ClearCurrentRenderTarget(bool color = true, bool depth = true)
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
        foreach (var glTex in _texturesToDispose) glTex.Dispose();
        foreach (var dev in _rboFboToDispose) dev.Dispose();
        
        //Utils.DisposeAllFields(this);
        
        unsafe { GLFW.DestroyWindow(_window); }
        GLFW.Terminate();
    }
}