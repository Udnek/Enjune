using Enjune.Graphic.GraphicApi;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.Graphic.InputHandler;

public class BasicInputHandler : IUserInputHandler
{
    private readonly IGraphicApi _graphicApi;
    private readonly KeyBinds _binds;
    private readonly ISet<KeyBinds.Bind> _pressed = new HashSet<KeyBinds.Bind>();
    private readonly ISet<KeyBinds.Bind> _shortPressed = new HashSet<KeyBinds.Bind>();
    private bool _firstCursorMove = true;
    private Vector2d _previousMousePosition = (0, 0);
    public Vector2d DeltaMousePosition { private set; get; } = (0, 0);

    public BasicInputHandler(IGraphicApi graphicApi, KeyBinds binds)
    {
        _graphicApi = graphicApi;
        _binds = binds;
    }

    public void HandleKey(GlfwKey key, IGraphicApi.KeyAction action)
    {
        var bind = _binds.Get(key);
        if (bind == null) return;
        if (bind.ContinuousPress)
        {
            if (action == IGraphicApi.KeyAction.Press) 
                _pressed.Add(bind);
            else if (action == IGraphicApi.KeyAction.Release) 
                _pressed.Remove(bind);
        } else 
        {
            if (action is IGraphicApi.KeyAction.Press or IGraphicApi.KeyAction.Repeat)
                _shortPressed.Add(bind);
        }
    }
    
    public void HandleCursor(double x, double y)
    {
        
        if (_firstCursorMove)
        {
            _previousMousePosition = (x, y);
            _firstCursorMove = false;
            return;
        }
        // we += cause this function will be called several times between frames
        DeltaMousePosition += (x, y) - _previousMousePosition;
        _previousMousePosition = (x, y);
        // already returns delta
        // if (_graphicApi.GetCursorMode() == IGraphicApi.CursorMode.Centered)
        // {
        //     // we += cause this function will be called several times between frames
        //     Logger.Log(this, $"x: {x}, y: {y}");
        //     DeltaMousePosition += (x, y);
        // }
        // else
        // {
        //
        // }
    }

    public bool IsPressed(KeyBinds.Bind bind) => _pressed.Contains(bind) || _shortPressed.Contains(bind);

    public void ClearForNextFrame()
    {
        _shortPressed.Clear();
        DeltaMousePosition = (0, 0);
    }
}