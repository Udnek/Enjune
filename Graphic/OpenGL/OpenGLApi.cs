// Main OpenGL API implementation

using Engine.Graphic.KeyHandler;
using Engine.Graphic.OpenGL.Arrays;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Engine.Graphic.OpenGL;

public class OpenGlApi : IGraphicApi
{
    private unsafe Window* _window;   
    private VAO _vao = null!;
    private VBO _vbo = null!;
    private EBO _ebo = null!;
    private ShaderProgram _shaderProgram = null!;

    private readonly List<float> _vertices = [];
    private readonly List<int> _elements = [];
    // todo delete probably?
    private int _elementsAmount = 0;

    public void Init(int width, int height, string title,
        IUserInputHandler keyHandler,
        IGraphicApi.WindowSizeChangeHandler windowSizeHandler)
    {
        // Setup error callback
        GLFW.SetErrorCallback((error, description) =>
        {
            Console.Error.WriteLine($"{error}: {description}");
        });

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
            GLFW.SetKeyCallback(_window, (window, key, scancode, glAction, mods) =>
            {
                if (key == GlfwKey.Escape && glAction == InputAction.Release)
                    GLFW.SetWindowShouldClose(window, true);

                var action = glAction switch
                {
                    InputAction.Release => IGraphicApi.KeyAction.Release,
                    InputAction.Press   => IGraphicApi.KeyAction.Press,
                    InputAction.Repeat  => IGraphicApi.KeyAction.Repeat,
                    _ => throw new Exception($"Unknown key action: {glAction}")
                };
                keyHandler.Handle(key, action);
            });

            // Framebuffer size callback
            GLFW.SetFramebufferSizeCallback(_window, (_, newWidth, newHeight) =>
            {
                windowSizeHandler(newWidth, newHeight);
            });
            
            GLFW.MakeContextCurrent(_window);
            GLFW.SwapInterval(1); // enable vsync
            GLFW.ShowWindow(_window);  
        }
        
        // enable features
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // load textures
        LoadTexture();

        // other objects
        _vao = new VAO();
        _vbo = new VBO();
        _ebo = new EBO();
        _shaderProgram = new ShaderProgram();
    }

    private void LoadTexture()
    {
        // TODO
    }

    public void UpdateEvents() => GLFW.PollEvents();

    public void Title(string title)
    {
        unsafe
        {
            GLFW.SetWindowTitle(_window, title);
        }
    }

    public void ViewPort(int x, int y, int width, int height) => GL.Viewport(x, y, width, height);

    public void ClearColor(Color color) => GL.ClearColor(color.X, color.Y, color.Z, color.W);

    public bool ShouldStop()
    {
        unsafe 
        {
            return GLFW.WindowShouldClose(_window);
        }
    } 

    public void PutVertex(Vector3 v, Color color)
    {
        _vertices.Add(v.X);
        _vertices.Add(v.Y);
        _vertices.Add(v.Z);
        _vertices.Add(color.X);
        _vertices.Add(color.Y);
        _vertices.Add(color.Z);
        _vertices.Add(color.W);
        
        _elements.Add(_elementsAmount);
        _elementsAmount++;
    }

    public void Model(Matrix4 model) => _shaderProgram.Model.SetValue(model);
    public void Projection(Matrix4 proj) => _shaderProgram.Projection.SetValue(proj);
    public void View(Matrix4 view) => _shaderProgram.View.SetValue(view);

    public void ClearRenderBuffer() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    public void Render()
    {
        // load
        _vbo.BindAndPut(_vertices.ToArray());
        _ebo.BindAndPut(_elements.ToArray());
        
        // draw
        GL.DrawElements(PrimitiveType.Triangles, _elementsAmount, DrawElementsType.UnsignedInt, 0);

        // clear
        _vertices.Clear();
        _elements.Clear();
        _elementsAmount = 0;

        // swap buffers
        unsafe
        {
            GLFW.SwapBuffers(_window);
        }
    }

    public void Destroy()
    {
        _vao.Destroy();
        _vbo.Destroy();
        _ebo.Destroy();
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