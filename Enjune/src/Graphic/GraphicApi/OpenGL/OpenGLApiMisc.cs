using Enjune.File;
using Enjune.Misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Enjune.Graphic.GraphicApi.OpenGL;

public sealed partial class OpenGlApi
{
    public void DumpTextures(ExternalPath path) => _textureArray.Dump(path);

    public void Title(string title)
    {
        unsafe
        {
            GLFW.SetWindowTitle(_window, title);
        }
    }

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
            case IGraphicApi.DrawMode.Point:
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Point);
                break;
            default:
                Logger.Error(this, $"unknown graphic mode: {mode}");
                break;
        }
    }

    private PrimitiveType fromApi(IGraphicApi.Primitive primitive)
    {
        return primitive switch
        {
            IGraphicApi.Primitive.Triangle => PrimitiveType.Triangles,
            IGraphicApi.Primitive.LineStrip => PrimitiveType.LineStrip,
            IGraphicApi.Primitive.LineLoop => PrimitiveType.LineLoop,
            IGraphicApi.Primitive.Line => PrimitiveType.Lines,
            IGraphicApi.Primitive.Point => PrimitiveType.Points,
            _ => throw new ArgumentOutOfRangeException(nameof(primitive), primitive, null)
        };
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

    public void ModelTransform(Matrix4 model) => _model.SetValue(_currentShader, model);
    public void ProjectionTransform(Matrix4 proj) => _projection.SetValue(_currentShader, proj);
    public void ViewTransform(Matrix4 view) => _view.SetValue(_currentShader, view);
    public void ViewPosition(Position position) => _viewPos.SetValue(_currentShader, position);
    public void GlobalColor(Color color) => _globalColor.SetValue(_currentShader, color);

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
    
    public void UpdateScreen() { unsafe { GLFW.SwapBuffers(_window); } }
    
    public void ClearScreenBuffers(bool color, bool depth)
    {
        GL.Clear(
            (color ? ClearBufferMask.ColorBufferBit : 0) 
            | (depth ? ClearBufferMask.DepthBufferBit : 0));
    }
}