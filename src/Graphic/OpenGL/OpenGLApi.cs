// Main OpenGL API implementation

using System.Runtime.InteropServices;
using Enjune.Graphic.InputHandler;
using Enjune.Graphic.OpenGL.Arrays;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;

namespace Enjune.Graphic.OpenGL;

public sealed class OpenGLApi : IGraphicApi
{
    private unsafe Window* _window;
    private VAO _vao = null!;
    private VBO _vbo = null!;
    private EBO _ebo = null!;
    private ShaderProgram _shaderProgram = null!;
    
    // so fucking gc won't erase it
    private GLFWCallbacks.KeyCallback _keyCallback = null!;
    private GLFWCallbacks.FramebufferSizeCallback _sizeChangeCallback = null!;

    private static readonly int VboCapacity = (int) Math.Pow(2, 25);
    private static readonly int EboCapacity = (int) Math.Pow(2, 22);
    
    
    public void Init(int width, int height, string title,
        IUserInputHandler keyHandler,
        IGraphicApi.WindowSizeChangeHandler windowSizeHandler)
    {
        // Setup error callback
        GLFW.SetErrorCallback((error, description) => { Console.Error.WriteLine($"{error}: {description}"); });
        
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

        void DebugProc(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr messagePointer, IntPtr param)
        {
            string message = Marshal.PtrToStringUTF8(messagePointer, length);
            Console.WriteLine(message);
            if (type == DebugType.DebugTypeError) throw new Exception(message);
        }
        GL.DebugMessageCallback(DebugProc, IntPtr.Zero);

        // other objects
        _vao = new VAO();
        _vbo = new VBO(VboCapacity);
        _ebo = new EBO(EboCapacity);
        _shaderProgram = new ShaderProgram();
        
        // load textures
        InitTexture();
    }

    private void InitTexture()
    {
        int textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        
        StbImage.stbi_set_flip_vertically_on_load(1);
        var atlas = FileManager.LoadAtlas();
        GL.TexImage2D(TextureTarget.Texture2D, 0, 
            PixelInternalFormat.Rgba, 
            atlas.Width, atlas.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, atlas.Data);
        
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public void UpdateEvents() => GLFW.PollEvents();

    public void Title(string title)
    {
        unsafe { GLFW.SetWindowTitle(_window, title); }
    }

    public void ViewPort(int x, int y, int width, int height) => GL.Viewport(x, y, width, height);

    public void SetClearColor(Color color) => GL.ClearColor(color.X, color.Y, color.Z, color.W);

    public void Model(Matrix4 model) => _shaderProgram.Model.SetValue(model);
    public void Projection(Matrix4 proj) => _shaderProgram.Projection.SetValue(proj);
    public void View(Matrix4 view) => _shaderProgram.View.SetValue(view);

    public bool ShouldStop()
    {
        unsafe { return GLFW.WindowShouldClose(_window); }
    }
    
    public void ClearVerticesBuffers()
    {
        _vbo.Clear();
        _ebo.Clear();
    }

    public void PutVertex(Position v, Color color, TextureCoord texCoord)
    {
        _vbo.Put(v.X);
        _vbo.Put(v.Y);
        _vbo.Put(v.Z);
        
        _vbo.Put(color.X);
        _vbo.Put(color.Y);
        _vbo.Put(color.Z);
        _vbo.Put(color.W);
        
        _vbo.Put(texCoord.X);
        _vbo.Put(texCoord.Y);

        _ebo.Put(_ebo.Count);
    }
    
    public void ClearScreenBuffers() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    
    public void RenderToScreenBuffer()
    {
        // load
        _vbo.BindAndPush();
        _ebo.BindAndPush();

        // draw
        GL.DrawElements(PrimitiveType.Triangles, _ebo.Count, DrawElementsType.UnsignedInt, 0);
    }

    public void UpdateScreen() { unsafe { GLFW.SwapBuffers(_window); } }

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