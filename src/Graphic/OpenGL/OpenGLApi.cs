// Main OpenGL API implementation

using System.Runtime.InteropServices;
using Enjune.Graphic.InputHandler;
using Enjune.Graphic.OpenGL.Array;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Enjune.Graphic.OpenGL;

public sealed class OpenGLApi : IGraphicApi
{
    private unsafe Window* _window;
    
    private Vao _vaoColored = null!;
    private Vao _vaoWhite = null!;
    private VboAndEbo _vboAndEbo = null!;
    private ShaderProgram _shaderProgram = null!;
    
    private static readonly int VboCapacity = (int) Math.Pow(2, 23);
    private static readonly int EboCapacity = (int) Math.Pow(2, 23);
    
    private readonly SmartBuffer<float> _vboWhiteBuffer = new (VboCapacity);
    private readonly SmartBuffer<float> _vboColoredBuffer = new (VboCapacity);
    private readonly SmartBuffer<int> _eboWhiteBuffer = new (EboCapacity);
    private readonly SmartBuffer<int> _eboColoredBuffer = new (EboCapacity);
    
    // so fucking gc won't erase it
    private GLFWCallbacks.KeyCallback _keyCallback = null!;
    private GLFWCallbacks.FramebufferSizeCallback _sizeChangeCallback = null!;
    private DebugProc _debugProc = null!;
    
    public void Init(int width, int height, string title,
        IUserInputHandler keyHandler,
        IGraphicApi.WindowSizeChangeHandler windowSizeHandler)
    {
        // Setup error callback
        GLFW.SetErrorCallback((error, description) => { Logger.Error($"{error}: {description}"); });
        
        if (!GLFW.Init())
            throw new Exception("Unable to initialize GLFW");

        // Configure GLFW
        GLFW.DefaultWindowHints();
        GLFW.WindowHint(WindowHintBool.Visible, true);
        GLFW.WindowHint(WindowHintBool.Resizable, true);
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 3);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 2);
        GLFW.WindowHint(WindowHintBool.OpenGLForwardCompat, true);

        // Create window
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
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.Enable(EnableCap.DebugOutput);

        _debugProc = (source, type, id, severity, length, messagePointer, param) =>
        {
            if (type != DebugType.DebugTypeError) return;
            string message = Marshal.PtrToStringUTF8(messagePointer, length);
            throw new Exception(message);
        };
        GL.DebugMessageCallback(_debugProc, IntPtr.Zero);
        
        // other objects
        _vboAndEbo = new VboAndEbo(BufferUsageHint.DynamicDraw);
        _shaderProgram = new ShaderProgram();
        InitVaos();
    }

    private const string AttributePosition = "position";
    private const string AttributeColor = "color";
    private const string AttributeTexture = "texcoord";
    
    private void InitVaos()
    {
        {
            _vaoColored = new Vao();
            var attributes = new VaoAttributes(_shaderProgram);
            attributes.Add(new Attribute(AttributePosition, 3));
            attributes.Add(new Attribute(AttributeColor, 4));
            attributes.Add(new Attribute(AttributeTexture, 2));
            attributes.Compile();
        }
        {
            _vaoWhite = new Vao();
            var attributes = new VaoAttributes(_shaderProgram);
            attributes.Add(new Attribute(AttributePosition, 3));
            // no color here
            attributes.Add(new Attribute(AttributeTexture, 2));
            attributes.Compile();
        }
    }

    public void UpdateEvents() => GLFW.PollEvents();

    public void Title(string title) { unsafe { GLFW.SetWindowTitle(_window, title); } }

    public void ViewPort(int x, int y, int width, int height) => GL.Viewport(x, y, width, height);

    public void SetClearColor(Color color) => GL.ClearColor(color.X, color.Y, color.Z, color.W);

    public void Model(Matrix4 model) => _shaderProgram.Model.SetValue(model);
    public void Projection(Matrix4 proj) => _shaderProgram.Projection.SetValue(proj);
    public void View(Matrix4 view) => _shaderProgram.View.SetValue(view);

    public bool ShouldStop() { unsafe { return GLFW.WindowShouldClose(_window); } }
    
    public void ClearVerticesBuffers()
    {
        _vboColoredBuffer.Clear();
        _eboColoredBuffer.Clear();
        
        _vboWhiteBuffer.Clear();
        _eboWhiteBuffer.Clear();
    }

    public void PutColoredMesh(Mesh mesh, Color color)
    {
        var offset = _vboColoredBuffer.Count / (3+4+2);
        foreach (var meshIndex in mesh.Indexes)
        {
            _eboColoredBuffer.Put(offset + meshIndex);
        }

        for (var i = 0; i < mesh.Vertices.Length; i++)
        {
            var vertex = mesh.Vertices[i];
            _vboColoredBuffer.Put(vertex.X);
            _vboColoredBuffer.Put(vertex.Y);
            _vboColoredBuffer.Put(vertex.Z);

            _vboColoredBuffer.Put(color.X);
            _vboColoredBuffer.Put(color.Y);
            _vboColoredBuffer.Put(color.Z);
            _vboColoredBuffer.Put(color.W);

            _vboColoredBuffer.Put(mesh.Textures[i].X);
            _vboColoredBuffer.Put(mesh.Textures[i].Y);
        }
    }

    public void PutWhiteVertex(Position v, TextureCoord texCoord)
    {
        _vboWhiteBuffer.Put(v.X);
        _vboWhiteBuffer.Put(v.Y);
        _vboWhiteBuffer.Put(v.Z);
        
        _vboWhiteBuffer.Put(texCoord.X);
        _vboWhiteBuffer.Put(texCoord.Y);

        _eboWhiteBuffer.Put(_eboWhiteBuffer.Count);
    }
    
    public void PutColoredVertex(Position v, Color color, TextureCoord texCoord)
    {
        _vboColoredBuffer.Put(v.X);
        _vboColoredBuffer.Put(v.Y);
        _vboColoredBuffer.Put(v.Z);
        
        _vboColoredBuffer.Put(color.X);
        _vboColoredBuffer.Put(color.Y);
        _vboColoredBuffer.Put(color.Z);
        _vboColoredBuffer.Put(color.W);
        
        _vboColoredBuffer.Put(texCoord.X);
        _vboColoredBuffer.Put(texCoord.Y);

        _eboColoredBuffer.Put(_eboColoredBuffer.Count);
    }
    
    public void ClearScreenBuffers() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    
    public void RenderToScreenBuffer()
    {
        // colored
        _shaderProgram.ColorProvided.SetValue(true);
        _vaoColored.Bind();
        _vboAndEbo.PushVbo(_vboColoredBuffer);
        _vboAndEbo.PushEbo(_eboColoredBuffer);
        //Logger.Log("colored size: " + _vboColoredBuffer.Count);
        GL.DrawElements(PrimitiveType.Triangles, _eboColoredBuffer.Count, DrawElementsType.UnsignedInt, 0);
        
        // white
        _shaderProgram.ColorProvided.SetValue(false);
        _vaoWhite.Bind();
        _vboAndEbo.PushVbo(_vboWhiteBuffer);
        _vboAndEbo.PushEbo(_eboWhiteBuffer);
        //Logger.Log("white size: " + _vboWhiteBuffer.Count);
        GL.DrawElements(PrimitiveType.Triangles, _eboWhiteBuffer.Count, DrawElementsType.UnsignedInt, 0);
    }

    public void UpdateScreen() { unsafe { GLFW.SwapBuffers(_window); } }

    public void Destroy()
    {
        _vboAndEbo.Destroy();
        
        _vaoColored.Destroy();
        _vaoWhite.Destroy();
        
        _shaderProgram.Destroy();

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