using Enjune.Graphic.GraphicApi;
using OpenTK.Mathematics;

namespace Enjune.Graphic.Input;

public class BasicInputHandler : IUserInputHandler
{
    public readonly KeyBinds Binds;
    
    private readonly HashSet<KeyBinds.Bind> _pressed = [];
    private readonly HashSet<KeyBinds.Bind> _shortPressed = [];
    private readonly HashSet<KeyBinds.Bind> _justReleased = [];
    
    private bool _firstCursorMove = true;
    public Vector2i CursorPosition = (0, 0);
    public Vector2i DeltaCursorPosition { get; private set; } = (0, 0);
    public Vector2 DeltaWheelScroll { get; private set; } = (0, 0);
    public Vector2i WindowSize { get; private  set; }
    public bool WindowSizeChanged { get; private set; } = false;

    public BasicInputHandler(KeyBinds binds, Vector2i initialWindowSize)
    {
        Binds = binds;
        WindowSize = initialWindowSize;
    }

    public void HandleWindowSizeChange(Vector2i newSize)
    {
        WindowSize = newSize;
        WindowSizeChanged = true;
    }

    public void HandleKey(KeyCode keyCode, IGraphicApi.KeyAction action)
    {
        if (!Binds.TryGet(keyCode, out var bind))
            return;
        
        if (bind!.ContinuousPress)
        {
            if (action == IGraphicApi.KeyAction.Press) 
                _pressed.Add(bind);
            else if (action == IGraphicApi.KeyAction.Release)
            {
                _justReleased.Add(bind);
                _pressed.Remove(bind);
            }
        } 
        else 
        {
            if (action is IGraphicApi.KeyAction.Press or IGraphicApi.KeyAction.Repeat)
                _shortPressed.Add(bind);
            else
                _justReleased.Add(bind);
        }
    }

    public void HandleCursor(int x, int y)
    {
        if (_firstCursorMove)
        {
            CursorPosition = (x, y);
            _firstCursorMove = false;
            return;
        }
        // we += cause this function will be called several times between frames
        DeltaCursorPosition += (x, y) - CursorPosition;
        CursorPosition = (x, y);
    }

    public void HandleScroll(float x, float y) => DeltaWheelScroll += (x, y);

    public bool IsPressed(KeyBinds.Bind bind) => _pressed.Contains(bind) || _shortPressed.Contains(bind);
    public bool IsJustReleased(KeyBinds.Bind bind) => _justReleased.Contains(bind);
    
    public void ClearForNextFrame()
    {
        _shortPressed.Clear();
        _justReleased.Clear();
        DeltaCursorPosition = (0, 0);
        DeltaWheelScroll = (0, 0);
        WindowSizeChanged = false;
    }
}