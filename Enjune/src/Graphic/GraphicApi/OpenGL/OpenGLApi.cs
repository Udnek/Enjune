using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Graphic.Asset;
using Enjune.Graphic.GraphicApi.OpenGL.Component;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Array;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Texture;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;
using Enjune.Graphic.GraphicApi.OpenGL.Shader;
using Enjune.Graphic.GraphicApi.Vertex.Colored;
using Enjune.Graphic.GraphicApi.Vertex.Material;
using Enjune.Misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using BeginMode = OpenTK.Graphics.OpenGL.BeginMode;

namespace Enjune.Graphic.GraphicApi.OpenGL;

public sealed partial class OpenGlApi : GLDisposable, IGraphicApi
{
    private unsafe Window* _window;
    
    private CompiledAssets _assets = null!;
    
    // buffers
    private Vao _mainVao = null!;
    private Vbo<MaterialVertexData> _matVertexVbo = null!;
    private Ssbo<MatId> _matIdSsbo = null!;
    private Ssbo<MaterialData> _materialSsbo = null!;
    private Ebo _ebo = null!;
    
    private Vao _colorVao = null!;
    private Vbo<ColoredVertexData> _colorVertexVbo = null!;

    private TextureArray _textureArray = null!;
    
    // shaders
    private MaterialShader _materialShader = null!;
    private ColorShader _colorShader = null!;
    
    // storing it here fucking gc won't erase it
    private GLFWCallbacks.KeyCallback _keyCallback = null!;
    private GLFWCallbacks.CursorPosCallback _cursorCallback = null!;
    private GLFWCallbacks.MouseButtonCallback _mouseButtonCallback = null!;
    private GLFWCallbacks.FramebufferSizeCallback _sizeChangeCallback = null!;
    private DebugProc _debugProc = null!;
    
    
    public Error? Init(CompiledAssets assets, int width, int height, string title, int verticesCapacity,
        IUserInputHandler keyHandler,
        IGraphicApi.WindowSizeChangeHandler windowSizeHandler)
    {
        _assets = assets;
        
        // error callback
        GLFW.SetErrorCallback((error, description) =>
        {
            Logger.Error(this, $"{error}: {description}");
        });

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
            
            _keyCallback = (window, key, scancode, glAction, mods) => keyHandler.HandleKey(key, FromGlwf(glAction));
            GLFW.SetKeyCallback(_window, _keyCallback);
            
            _mouseButtonCallback = (window, button, action, mods) => keyHandler.HandleMouseKey(button, FromGlwf(action));
            GLFW.SetMouseButtonCallback(_window, _mouseButtonCallback);
            
            _sizeChangeCallback = (_, newWidth, newHeight) => windowSizeHandler(newWidth, newHeight);
            GLFW.SetFramebufferSizeCallback(_window, _sizeChangeCallback);

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
            {
                Logger.Error(this, message);
                throw new Exception(message);  
            }
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
        _textureArray = new TextureArray(_assets, TextureUnit.Texture0);
        _mainVao = new Vao();
        _materialSsbo = new Ssbo<MaterialData>(0, _assets.Materials.Length);
        _matIdSsbo = new Ssbo<MatId>(1, verticesCapacity);
        _matVertexVbo = new Vbo<MaterialVertexData>(verticesCapacity);
        _ebo = new Ebo(verticesCapacity);
        _colorVao = new Vao();
        _colorVertexVbo = new Vbo<ColoredVertexData>(verticesCapacity);
        
        // loading shaders
        _materialShader = new MaterialShader(_mainVao, _matVertexVbo, _matIdSsbo, _ebo, 0);
        var error = _materialShader.Init(
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Material", "frag.frag"),
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Material", "vert.vert"),
            a => a
                .Add<float>(VertexAttribPointerType.Float, "aPos", 3)
                .Add<float>(VertexAttribPointerType.Float, "aTexPos", 2)
                .Add<float>(VertexAttribPointerType.Float, "aNorm", 3)
            );
        if (error != null) return error;
        
        _colorShader = new ColorShader(_colorVao, _colorVertexVbo, _ebo);
        error = _colorShader.Init(
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Color", "frag.frag"),
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Color", "vert.vert"),
            a => a
                .Add<float>(VertexAttribPointerType.Float, "aPos", 3)
                .Add<float>(VertexAttribPointerType.Float, "aColor", 4)
            );
        if (error != null) return error;
        
        // loading materials
        {
            MaterialData ToData(CompiledMaterial mat) => new(mat.Raw.Color, mat.TextureId);
            var matBuffer = new FixedBuffer<MaterialData>(_assets.Materials.Length);
            matBuffer.Put(_assets.Materials.Select(ToData).ToArray());
            _materialSsbo.BindAndPush(matBuffer);
        }
        
        return null;
    }

    public void UseShader<T>(Consumer<T> consumer) where T : IShader
    {
        BaseShader shader;
        if (typeof(T) == typeof(IShader.IMaterial))
            shader = _materialShader;
        else if (typeof(T) == typeof(IShader.IColor))
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


    protected override void DisposeGLData()
    {
        _mainVao.Dispose();
        _colorVao.Dispose();
        
        _materialSsbo.Dispose();
        _matIdSsbo.Dispose();
        
        _matVertexVbo.Dispose();
        _colorVertexVbo.Dispose();
        
        _ebo.Dispose();
        _materialShader.Dispose();
        _colorShader.Dispose();
        
        _textureArray.Dispose();
        
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