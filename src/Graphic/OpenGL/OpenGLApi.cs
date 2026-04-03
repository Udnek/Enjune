using System.Runtime.InteropServices;
using Enjune.File;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.GraphicApi.Data;
using Enjune.Graphic.InputHandler;
using Enjune.Graphic.OpenGL.Component;
using Enjune.Graphic.OpenGL.Component.Array;
using Enjune.Graphic.OpenGL.Component.Texture;
using Enjune.Graphic.OpenGL.Component.Uniform;
using Enjune.Misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Enjune.Graphic.OpenGL;

public sealed class OpenGLApi : GLDisposable, IGraphicApi
{
    private unsafe Window* _window;
    
    private TextureManager _textureManager = null!;
    
    private Vao _vao = null!;
    private Vbo<VertexData> _vertexVbo = null!;
    private Vbo<MatId> _matIdVbo = null!;
    private Ebo _ebo = null!;
    
    private ShaderProgram _shaderProgram = null!;
    private TextureArray _textureArray = null!;
    
    // storing it here fucking gc won't erase it
    private GLFWCallbacks.KeyCallback _keyCallback = null!;
    private GLFWCallbacks.FramebufferSizeCallback _sizeChangeCallback = null!;
    private DebugProc _debugProc = null!;
    
    // uniforms
    private BoolUniform _colorProvided = null!;
    private Matrix4Uniform _model = null!;
    private Matrix4Uniform _view = null!;
    private Matrix4Uniform _projection = null!;
    private TextureUniform _textureUniform = null!;

    public void Init(TextureManager textureManager, int width, int height, string title,
        IUserInputHandler keyHandler,
        IGraphicApi.WindowSizeChangeHandler windowSizeHandler)
    {
        _textureManager = textureManager;
        
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
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 3);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 2);
        GLFW.WindowHint(WindowHintBool.OpenGLForwardCompat, true);

        // window creation
        unsafe
        {
            _window = GLFW.CreateWindow(width, height, title, null, null);
            if (_window == null)
                throw new Exception("Failed to create GLFW window");
            
            // keys callback
            _keyCallback = (window, key, scancode, glAction, mods) =>
            {
                if (key == GlfwKey.Escape && glAction == InputAction.Release)
                    GLFW.SetWindowShouldClose(window, true);

                var action = glAction switch
                {
                    InputAction.Release => IGraphicApi.KeyAction.Release,
                    InputAction.Press => IGraphicApi.KeyAction.Press,
                    InputAction.Repeat => IGraphicApi.KeyAction.Repeat,
                    _ => throw new Exception($"Unknown key action: {glAction}")
                };
                keyHandler.Handle(key, action);
            };
            GLFW.SetKeyCallback(_window, _keyCallback);

            // Framebuffer size callback
            _sizeChangeCallback = (_, newWidth, newHeight) => { windowSizeHandler(newWidth, newHeight); };
            GLFW.SetFramebufferSizeCallback(_window, _sizeChangeCallback);

            GLFW.MakeContextCurrent(_window);
            GLFW.SwapInterval(0);
            //GLFW.SwapInterval(1); // enable vsync
            GLFW.ShowWindow(_window);
        }
        
        // without that shit it won't work
        GL.LoadBindings(new GLFWBindingsContext());
        
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
        
        // other components init
        InitComponents();
        
        // just in case
        GL.GetInteger(GetPName.MaxTextureSize, out var maxTextureSize);
        GL.GetInteger(GetPName.MaxArrayTextureLayers, out var maxArrayLayers);
        Logger.Log(this, $"max possible texture size: {maxTextureSize}");
        Logger.Log(this, $"max possible texture array layers: {maxArrayLayers}");
    }

    
    private const string AttributePosition = "position";
    private const string AttributeColor = "color";
    private const string AttributeTexture = "texcoord";
    private const string AttributeTextureLayer = "texLayer";
    
    private const string UniformColorProvided = "colorProvided";
    private const string UniformTexture = "textureArray";
    private const string UniformModel = "model";
    private const string UniformView = "view";
    private const string UniformProjection = "projection";
    
    private void InitComponents()
    {
        _shaderProgram = new ShaderProgram(_textureManager, 
            new ResourcePath("OpenGL", "frag.frag"),
            new ResourcePath("OpenGL", "vert.vert"));
        _shaderProgram.Bind();
        
        // uniforms
        _colorProvided = new BoolUniform(_shaderProgram, UniformColorProvided);
        _model = new Matrix4Uniform(_shaderProgram, UniformModel);
        _view = new Matrix4Uniform(_shaderProgram, UniformView);
        _projection = new Matrix4Uniform(_shaderProgram, UniformProjection);
        _textureUniform = new TextureUniform(_shaderProgram, UniformTexture);
        
        // Set default projection matrix
        var defaultProjection = Matrix4.CreatePerspectiveFieldOfView(MathF.PI / 2, 1.0f, 0.1f, 1000.0f);
        _projection.SetValue(defaultProjection);
        
        // textures
        _textureArray = new TextureArray(_textureManager, TextureUnit.Texture0);
        
        // vaos
        _vao = new Vao();
        // todo better calc capacities
        _vertexVbo = new Vbo<VertexData>((int)Math.Pow(2, 20));
        _matIdVbo = new Vbo<MatId>((int)Math.Pow(2, 20));
        _ebo = new Ebo((int)Math.Pow(2, 20));
        
        {
            // attributes
            var attributes = new VaoAttributes<byte>(_vao, _vertexVbo, _shaderProgram);
            attributes.Add<float>(VertexAttribPointerType.Float, AttributePosition, 3);
            attributes.Add<float>(VertexAttribPointerType.Float, AttributeColor, 4);
            attributes.Add<float>(VertexAttribPointerType.Float, AttributeTexture, 2);
            attributes.Add<TexId>(VertexAttribPointerType.Int, AttributeTextureLayer, 1);
            attributes.Compile();
        }
        
        _shaderProgram.Bind();
    }

    public void DumpTextures() => _textureArray.Dump();

    public void UpdateEvents() => GLFW.PollEvents();

    public void Title(string title) { unsafe { GLFW.SetWindowTitle(_window, title); } }

    public void ViewPort(int x, int y, int width, int height) => GL.Viewport(x, y, width, height);

    public void SetClearColor(Color color) => GL.ClearColor(color.X, color.Y, color.Z, color.W);

    public void Model(Matrix4 model) => _model.SetValue(model);
    public void Projection(Matrix4 proj) => _projection.SetValue(proj);
    public void View(Matrix4 view) => _view.SetValue(view);

    public bool ShouldStop() { unsafe { return GLFW.WindowShouldClose(_window); } }
    
    public void ClearScreenBuffers() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    public void RenderToScreenBuffer<T>(VertexBuffer buffer) where T : unmanaged
    {
        if (buffer.ProvidesColor)
        {
            _colorProvided.SetValue(true);
            _vao.Bind();
        }
        else
        {
            _colorProvided.SetValue(false);
            _vaoWhite.Bind(); 
        }
        
        _vertexVbo.BindAndPush(buffer.Vbo);
        _ebo.BindAndPush(buffer.Ebo);
            
        GL.DrawElements(PrimitiveType.Triangles, buffer.Ebo.Count, DrawElementsType.UnsignedInt, 0);
    }
    
    public void UpdateScreen() { unsafe { GLFW.SwapBuffers(_window); } }
    
    protected override void DisposeGLData()
    {
        _vao.Dispose();
        _vaoWhite.Dispose();
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