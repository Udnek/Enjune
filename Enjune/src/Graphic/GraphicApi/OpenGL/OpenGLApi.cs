using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Graphic.Asset;
using Enjune.Graphic.GraphicApi.OpenGL.Component;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Array;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Texture;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;
using Enjune.Misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Enjune.Graphic.GraphicApi.OpenGL;

public sealed partial class OpenGlApi : GLDisposable, IGraphicApi
{
    private unsafe Window* _window;
    
    private CompiledAssets _assets = null!;
    
    private Vao _vao = null!;
    private Vbo<VertexData> _vertexVbo = null!;
    private Ssbo<MatId> _matIdSsbo = null!;
    private Ssbo<MaterialData> _materialSsbo = null!;
    private Ebo _ebo = null!;
    
    private ShaderProgram _mainShader = null!;
    private ShaderProgram _textShader = null!;
    private ShaderProgram _currentShader = null!;
    private TextureArray _textureArray = null!;
    
    // storing it here fucking gc won't erase it
    private GLFWCallbacks.KeyCallback _keyCallback = null!;
    private GLFWCallbacks.CursorPosCallback _cursorCallback = null!;
    private GLFWCallbacks.MouseButtonCallback _mouseButtonCallback = null!;
    private GLFWCallbacks.FramebufferSizeCallback _sizeChangeCallback = null!;
    private DebugProc _debugProc = null!;
    
    // uniforms
    private Matrix4Uniform _model = null!;
    private Matrix4Uniform _view = null!;
    private Matrix4Uniform _projection = null!;
    private Vector4Uniform _globalColor = null!;
    private TextureUniform _textureUniform = null!;

    public Error? Init(CompiledAssets assets, int width, int height, string title,
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
        GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
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
        return InitComponents();
    }
    
    private const string AttributePosition = "position";
    private const string AttributeTexture = "texcoord";
    
    private const string UniformTexture = "textureArray";
    private const string UniformModel = "model";
    private const string UniformView = "view";
    private const string UniformProjection = "projection";
    private const string UniformGlobalColor = "globalColor";
    
    private Error? InitComponents()
    {
        _mainShader = new ShaderProgram();
        var error = _mainShader.Init(
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Main", "frag.frag"),
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Main", "vert.vert"));
        if (error != null) return error;

        _textShader = new ShaderProgram();
        error = _textShader.Init(
            AssemblyPath.Of(Enjune.Assembly,"OpenGL", "Shaders", "Text", "frag.frag"),
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "Shaders", "Text", "vert.vert"));
        if (error != null) return error;
        
        // uniforms
        _model = new Matrix4Uniform(UniformModel, Matrix4.Identity, _mainShader, _textShader);
        _view = new Matrix4Uniform(UniformView, Matrix4.Identity, _mainShader, _textShader);
        _projection = new Matrix4Uniform(UniformProjection, 
            Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1.0f, 0.1f, 1000.0f), 
            _mainShader, _textShader);
        _textureUniform = new TextureUniform(UniformTexture, 0, _mainShader, _textShader);
        _globalColor = new Vector4Uniform(UniformGlobalColor, new Color(1f), _mainShader, _textShader);
        

        // textures
        _textureArray = new TextureArray(_assets, TextureUnit.Texture0);
        
        // buffers
        _vao = new Vao();
        // todo better calc capacities
        _materialSsbo = new Ssbo<MaterialData>(0, 20);
        _matIdSsbo = new Ssbo<MatId>(1, (int)Math.Pow(2, 20));
        _vertexVbo = new Vbo<VertexData>((int)Math.Pow(2, 20));
        _ebo = new Ebo((int)Math.Pow(2, 20));
        
        // attributes
        {
            var attributes = new VaoAttributes<VertexData>(_vao, _vertexVbo, _mainShader, _textShader);
            attributes.Add<float>(VertexAttribPointerType.Float, AttributePosition, 3);
            attributes.Add<float>(VertexAttribPointerType.Float, AttributeTexture, 2);
            attributes.Compile();
        }

        // loading materials
        {
            MaterialData ToData(CompiledMaterial mat) => new(mat.Raw.Color, mat.TextureId);
            var matBuffer = new FixedBuffer<MaterialData>(_assets.Materials.Length);
            matBuffer.Put(_assets.Materials.Select(ToData).ToArray());
            _materialSsbo.BindAndPush(matBuffer);
        }
        
        SelectShader(_mainShader);
        
        return null;
    }

    private void SelectShader(ShaderProgram shader)
    {
        if (_currentShader == shader) return;
        _currentShader = shader;
        shader.Bind();
    }
    
    public void SwitchShader(IGraphicApi.ShaderType type)
    {
        switch (type)
        {
            case IGraphicApi.ShaderType.Main:
                SelectShader(_mainShader);
                break;
            case IGraphicApi.ShaderType.Text:
                SelectShader(_textShader);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void RenderToScreenBuffer(VertexBuffer buffer)
    {
        _vertexVbo.BindAndPush(buffer.VertexVbo);
        _matIdSsbo.BindAndPush(buffer.MatIdSsbo);
        _ebo.BindAndPush(buffer.Ebo);

        GL.DrawElements(PrimitiveType.Triangles, buffer.Ebo.Count, DrawElementsType.UnsignedInt, 0);
    }

    protected override void DisposeGLData()
    {
        _vao.Dispose();
        _materialSsbo.Dispose();
        _matIdSsbo.Dispose();
        _vertexVbo.Dispose();
        _ebo.Dispose();
        _mainShader.Dispose();
        _textShader.Dispose();
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