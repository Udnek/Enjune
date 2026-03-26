// Main OpenGL API implementation

using System.Drawing;
using Engine.Graphic;
using Engine.Graphic.KeyHandler;
using Engine.Graphic.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Platform.Windows;
using OpenTK.Windowing.GraphicsLibraryFramework;
using All = OpenTK.Graphics.OpenGL.All;
using Monitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

public class OpenGLApi : GrApi
{
    private IntPtr _window;
    private VAO _vao = null!;
    private VBO _vbo = null!;
    private EBO _ebo = null!;
    private ShaderProgram _shaderProgram = null!;

    private readonly List<float> _vertices = [];
    private readonly List<int> _indices = [];
    private int _elementsAmount = 0;

    public void Init(int width, int height, string title,
                     UserInputHandler keyHandler,
                     GrApi.WindowSizeChangeHandler windowSizeHandler)
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
        _window = GLFW.CreateWindow(width, height, title, IntPtr.Zero, null);
        if (_window == IntPtr.Zero)
            throw new Exception("Failed to create GLFW window");

        // Key callback
        GLFW.SetKeyCallback(_window, (window, key, scancode, action, mods) =>
        {
            if (key == Keys.Escape && action == InputAction.Release)
                GLFW.SetWindowShouldClose(window, true);

            GrApi.KeyAction ka = action switch
            {
                InputAction.Release => GrApi.KeyAction.Release,
                InputAction.Press   => GrApi.KeyAction.Press,
                InputAction.Repeat  => GrApi.KeyAction.Repeat,
                _ => throw new Exception($"Unknown key action: {action}")
            };
            keyHandler(key, ka);
        });

        // Framebuffer size callback
        GLFW.SetFramebufferSizeCallback(_window, (window, width, height) =>
        {
            windowSizeHandler(width, height);
        });

        // Make context current and enable vsync
        GLFW.MakeContextCurrent(_window);
        GLFW.SwapInterval(1);
        GLFW.ShowWindow(_window);

        // Enable OpenGL features
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Load any textures (stub)
        LoadTexture();

        // Create OpenGL objects
        _vao = new VAO();
        _vbo = new VBO();
        _ebo = new EBO();

        // Set up vertex attribute layout
        _vao.Bind();
        _vbo.Bind();
        // Position attribute (location = 0)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        // Color attribute (location = 1)
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        _vao.Unbind();

        _shaderProgram = new ShaderProgram();
    }

    private void LoadTexture()
    {
        // Placeholder for texture loading logic
    }

    public void UpdateEvents() => GLFW.PollEvents();

    public void Title(string title) => GLFW.SetWindowTitle(_window, title);

    public void ViewPort(int x, int y, int width, int height) => GL.Viewport(x, y, width, height);

    public void ClearColor(Color color) => GL.ClearColor(color.X, color.Y, color.Z, color.W);

    public bool ShouldStop() => GLFW.WindowShouldClose(_window);

    public void PutVertex(Vector3 v, Color color)
    {
        _vertices.Add(v.X);
        _vertices.Add(v.Y);
        _vertices.Add(v.Z);
        _vertices.Add(color.X);
        _vertices.Add(color.Y);
        _vertices.Add(color.Z);
        _vertices.Add(color.W);
        _indices.Add(_elementsAmount);
        _elementsAmount++;
    }

    public void Model(Matrix4 model) => _shaderProgram.SetModel(model);
    public void Projection(Matrix4 proj) => _shaderProgram.SetProjection(proj);
    public void View(Matrix4 view) => _shaderProgram.SetView(view);

    public void ClearRenderBuffer() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

    public void Render()
    {
        // Upload vertex data
        if (_vertices.Count > 0)
        {
            _vbo.Bind();
            _vbo.SetData(_vertices.ToArray());
        }

        // Upload index data
        if (_indices.Count > 0)
        {
            _ebo.Bind();
            _ebo.SetData(_indices.ToArray());
        }

        // Use shader and draw
        _shaderProgram.Use();
        _vao.Bind();
        GL.DrawElements(PrimitiveType.Triangles, _elementsAmount, DrawElementsType.UnsignedInt, 0);
        _vao.Unbind();

        // Clear per-frame buffers
        _vertices.Clear();
        _indices.Clear();
        _elementsAmount = 0;

        // Swap buffers
        GLFW.SwapBuffers(_window);
    }

    public void Destroy()
    {
        _vao?.Dispose();
        _vbo?.Dispose();
        _ebo?.Dispose();
        _shaderProgram?.Dispose();

        GLFW.SetKeyCallback(_window, null);
        GLFW.SetFramebufferSizeCallback(_window, null);
        GLFW.DestroyWindow(_window);
        GLFW.Terminate();
        GLFW.SetErrorCallback(null);
    }
}

// Helper classes for OpenGL objects
public class VAO : IDisposable
{
    public int Handle { get; }

    public VAO()
    {
        Handle = GL.GenVertexArray();
        Bind();
    }

    public void Bind() => GL.BindVertexArray(Handle);
    public void Unbind() => GL.BindVertexArray(0);
    public void Dispose() => GL.DeleteVertexArray(Handle);
}

public class VBO : IDisposable
{
    public int Handle { get; }

    public VBO()
    {
        Handle = GL.GenBuffer();
    }

    public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
    public void Unbind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

    public void SetData(float[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
    {
        Bind();
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, usage);
    }

    public void Dispose() => GL.DeleteBuffer(Handle);
}

public class EBO : IDisposable
{
    public int Handle { get; }

    public EBO()
    {
        Handle = GL.GenBuffer();
    }

    public void Bind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
    public void Unbind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

    public void SetData(int[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
    {
        Bind();
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(int), data, usage);
    }

    public void Dispose() => GL.DeleteBuffer(Handle);
}