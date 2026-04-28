using System.Runtime.CompilerServices;
using Enjune.File;
using Enjune.Misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace Enjune.Graphic.GraphicApi.OpenGL;

public sealed partial class OpenGlApi
{

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
    
    public void DumpTextures(ExternalPath path)
    {
        _textureArray.Dump(path, "main")?.Log(this);
        _mainFbo.Texture.Dump(path, "screen")?.Log(this);
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