using System.Runtime.CompilerServices;
using Enjune.File;
using Enjune.Graphic.GraphicApi;
using Enjune.Graphic.Input;
using Enjune.Misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace OpenGLApi;

public sealed partial class OpenGlApi
{
    
    private readonly Dictionary<GlfwKey, KeyCode> _glfwKeyToKeyCode = new();
    private readonly Dictionary<MouseButton, KeyCode> _glfwMouseToKeyCode = new();

    public static Error? GetGlError()
    {
        var errorCode = GL.GetError();
        if (errorCode == ErrorCode.NoError) return null;
        return $"GL error: {Enum.GetName(errorCode)}";
    }

    public static void CheckGlError()
    {
        var error = GetGlError();
        if (error != null) throw new Exception(error);
    }

    private void InitKeyCodeMaps()
    {
        foreach (var code in Enum.GetValues<KeyCode>())
        {
            var either = Get(code);
            if (either is null)
            {
                Logger.Warn(this,$"no Glfw mapping for key: {code}");
                continue;
            }
            either.Map(k => _glfwKeyToKeyCode[k] = code, m => _glfwMouseToKeyCode[m] = code);
        }
        return;
        
        static Either<GlfwKey, MouseButton>? Get(KeyCode code)
        {
            return code switch
            {
                KeyCode.W => GlfwKey.W,
                KeyCode.A => GlfwKey.A,
                KeyCode.S => GlfwKey.S,
                KeyCode.D => GlfwKey.D,
                KeyCode.Space => GlfwKey.Space,
                KeyCode.F1 => GlfwKey.F1,
                KeyCode.F2 => GlfwKey.F2,
                KeyCode.F3 => GlfwKey.F3,
                KeyCode.F4 => GlfwKey.F4,
                KeyCode.F5 => GlfwKey.F5,
                KeyCode.F6 => GlfwKey.F6,
                KeyCode.F7 => GlfwKey.F7,
                KeyCode.F8 => GlfwKey.F8,
                KeyCode.F9 => GlfwKey.F9,
                KeyCode.F10 => GlfwKey.F10,
                KeyCode.F11 => GlfwKey.F11,
                KeyCode.F12 => GlfwKey.F12,
                KeyCode.LeftShift => GlfwKey.LeftShift,
                KeyCode.RightShift => GlfwKey.RightShift,
                KeyCode.Escape => GlfwKey.Escape,
                KeyCode.RightMouseButton => MouseButton.Right,
                KeyCode.LeftMouseButton => MouseButton.Left,
                _ => null
            };
        }
    }
    
    public void DumpTextures(ExternalPath path)
    {
        _textureArray.Dump(path, "main")?.Log(this);
        _screenPack.Texture.Dump(path, "screen")?.Log(this);
    }

    public void Title(string title)
    {
        unsafe
        {
            GLFW.SetWindowTitle(_window, title);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ViewPort(int width, int height) => GL.Viewport(0, 0, width, height);
    
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
            case IGraphicApi.DrawMode.Point:
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Point);
                break;
            default:
                Logger.Error(this, $"unknown graphic mode: {mode}");
                break;
        }
    }

    public void SetVsync(bool vsync) => GLFW.SwapInterval(vsync ? 1 : 0);

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

    public void SetWindowSize(Vector2i wh)
    {
        unsafe
        {
            GLFW.SetWindowSize(_window, wh.X, wh.Y);
        }
    }

    public Vector2i GetWindowSize()
    {
        unsafe
        {
            GLFW.GetWindowSize(_window, out int width, out int height);
            return new Vector2i(width, height);
        }
    }

    public bool ShouldStop()
    {
        unsafe
        {
            return GLFW.WindowShouldClose(_window);
        }
    }

    public void UpdateEvents() => GLFW.PollEvents();
}