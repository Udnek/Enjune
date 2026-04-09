using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Graphic.Asset;
using Enjune.Graphic.GraphicApi.OpenGL.Component;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Array;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Texture;
using Enjune.Graphic.GraphicApi.OpenGL.Component.Uniform;
using Enjune.Graphic.InputHandler;
using Enjune.Misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Enjune.Graphic.GraphicApi.OpenGL;

public sealed class OpenGLApi : GLDisposable, IGraphicApi
{
    private unsafe Window* _window;
    
    private CompiledAssets _assets = null!;
    
    private Vao _vao = null!;
    private Vbo<VertexData> _vertexVbo = null!;
    private Ssbo<MatId> _matIdSsbo = null!;
    private Ssbo<MaterialData> _materialSsbo = null!;
    private Ebo _ebo = null!;
    
    private ShaderProgram _shaderProgram = null!;
    private TextureArray _textureArray = null!;
    
    // storing it here fucking gc won't erase it
    private GLFWCallbacks.KeyCallback _keyCallback = null!;
    private GLFWCallbacks.CursorPosCallback _cursorCallback = null!;
    private GLFWCallbacks.FramebufferSizeCallback _sizeChangeCallback = null!;
    private DebugProc _debugProc = null!;
    
    // uniforms
    private Matrix4Uniform _model = null!;
    private Matrix4Uniform _view = null!;
    private Matrix4Uniform _projection = null!;
    private Vector4Uniform _globalColor = null!;
    private TextureUniform _textureUniform = null!;

    public void Init(CompiledAssets assets, int width, int height, string title,
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
            throw new Exception("Unable to initialize GLFW");

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
                throw new Exception("Failed to create GLFW window");
            
            // callbacks
            _keyCallback = (window, key, scancode, glAction, mods) =>
            {
                var action = glAction switch
                {
                    InputAction.Release => IGraphicApi.KeyAction.Release,
                    InputAction.Press => IGraphicApi.KeyAction.Press,
                    InputAction.Repeat => IGraphicApi.KeyAction.Repeat,
                    _ => throw new Exception($"Unknown key action: {glAction}")
                };
                keyHandler.HandleKey(key, action);
            };
            GLFW.SetKeyCallback(_window, _keyCallback);
            
            _sizeChangeCallback = (_, newWidth, newHeight) => windowSizeHandler(newWidth, newHeight);
            GLFW.SetFramebufferSizeCallback(_window, _sizeChangeCallback);

            _cursorCallback = (window, x, y) => keyHandler.HandleCursor(x, y);
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
            GLFW.SwapInterval(0); // enable vsync
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
        InitComponents();
    }

    
    private const string AttributePosition = "position";
    private const string AttributeTexture = "texcoord";
    private const string AttributeMaterialId = "materialId";
    
    private const string UniformTexture = "textureArray";
    private const string UniformModel = "model";
    private const string UniformView = "view";
    private const string UniformProjection = "projection";
    private const string UniformGlobalColor = "globalColor";
    
    private void InitComponents()
    {
        _shaderProgram = new ShaderProgram(
            AssemblyPath.Of(Enjune.Assembly,"OpenGL", "frag.frag"),
            AssemblyPath.Of(Enjune.Assembly, "OpenGL", "vert.vert"));
        
        _shaderProgram.Bind();
        
        // uniforms
        _model = new Matrix4Uniform(_shaderProgram, UniformModel);
        _view = new Matrix4Uniform(_shaderProgram, UniformView);
        _projection = new Matrix4Uniform(_shaderProgram, UniformProjection);
        _textureUniform = new TextureUniform(_shaderProgram, UniformTexture);
        _globalColor = new Vector4Uniform(_shaderProgram, UniformGlobalColor);
        
        // Set default for uniforms
        {
            _globalColor.SetValue(new Color(1));
            var defaultProjection = Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1.0f, 0.1f, 1000.0f);
            _projection.SetValue(defaultProjection);  
        }
        
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
            var attributes = new VaoAttributes<VertexData>(_vao, _vertexVbo, _shaderProgram);
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
    }

    public void DumpTextures(ExternalPath path) => _textureArray.Dump(path);

    public void UpdateEvents() => GLFW.PollEvents();

    public void Title(string title) { unsafe { GLFW.SetWindowTitle(_window, title); } }

    public void ViewPort(int x, int y, int width, int height) => GL.Viewport(x, y, width, height);

    public void SetClearColor(Color color) => GL.ClearColor(color.X, color.Y, color.Z, color.W);
    public void SetDrawMode(IGraphicApi.DrawMode mode)
    {
        switch (mode)
        {
            case IGraphicApi.DrawMode.Fill:
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
                break;
            case IGraphicApi.DrawMode.Wireframe:
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
                break;
            default:
                Logger.Error(this, $"unknown graphic mode: {mode}");
                break;
        }
    }

    public IGraphicApi.CursorMode GetCursorMode()
    {
        unsafe
        {
            return GLFW.GetInputMode(_window, CursorStateAttribute.Cursor) switch
            {
                CursorModeValue.CursorNormal => IGraphicApi.CursorMode.Normal,
                CursorModeValue.CursorHidden => IGraphicApi.CursorMode.Invisible,
                CursorModeValue.CursorDisabled => IGraphicApi.CursorMode.Centered,
                CursorModeValue.CursorCaptured => IGraphicApi.CursorMode.CanNotLeaveWindow,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public void SetCursorMode(IGraphicApi.CursorMode mode)
    {
        var state = mode switch
        {
            IGraphicApi.CursorMode.Normal => CursorModeValue.CursorNormal,
            IGraphicApi.CursorMode.Invisible => CursorModeValue.CursorHidden,
            IGraphicApi.CursorMode.Centered => CursorModeValue.CursorDisabled,
            IGraphicApi.CursorMode.CanNotLeaveWindow => CursorModeValue.CursorCaptured,
            _ => throw new ArgumentOutOfRangeException()
        };
        unsafe
        {
            GLFW.SetInputMode(_window, CursorStateAttribute.Cursor, state);
        }
    }

    public void ModelTransform(Matrix4 model) => _model.SetValue(model);
    public void ProjectionTransform(Matrix4 proj) => _projection.SetValue(proj);
    public void ViewTransform(Matrix4 view) => _view.SetValue(view);
    public void GlobalColor(Color color) =>  _globalColor.SetValue(color);

    public bool ShouldStop() { unsafe { return GLFW.WindowShouldClose(_window); } }
    
    public void ClearScreenBuffers() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    public void RenderToScreenBuffer(VertexBuffer buffer)
    {
        _vertexVbo.BindAndPush(buffer.VertexVbo);
        _matIdSsbo.BindAndPush(buffer.MatIdSsbo);
        _ebo.BindAndPush(buffer.Ebo);

        GL.DrawElements(PrimitiveType.Triangles, buffer.Ebo.Count, DrawElementsType.UnsignedInt, 0);
    }
    
    public void UpdateScreen() { unsafe { GLFW.SwapBuffers(_window); } }
    
    protected override void DisposeGLData()
    {
        _vao.Dispose();
        _materialSsbo.Dispose();
        _matIdSsbo.Dispose();
        _vertexVbo.Dispose();
        _ebo.Dispose();
        _shaderProgram.Dispose();
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