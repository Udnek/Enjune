using Enjune.Graphic.GraphicApi;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Enjune.Graphic.Input;

public class BasicInputHandler : IUserInputHandler
{
    private readonly IGraphicApi _graphicApi;
    public readonly KeyBinds Binds;
    
    private readonly HashSet<KeyBinds.Bind> _pressed = [];
    private readonly HashSet<KeyBinds.Bind> _shortPressed = [];
    private readonly HashSet<KeyBinds.Bind> _justReleased = [];
    
    private bool _firstCursorMove = true;
    public Vector2i CursorPosition = (0, 0);
    public Vector2i DeltaCursorPosition { private set; get; } = (0, 0);

    public BasicInputHandler(IGraphicApi graphicApi, KeyBinds binds)
    {
        _graphicApi = graphicApi;
        Binds = binds;
    }

    public void HandleKey(GlfwKey key, IGraphicApi.KeyAction action) => Handle(UniKey.Of(key), action);
    
    public void HandleMouseKey(MouseButton key, IGraphicApi.KeyAction action) => Handle(UniKey.Of(key), action);

    private void Handle(UniKey uniKey, IGraphicApi.KeyAction action)
    {
        if (!Binds.TryGet(uniKey, out var bind))
            return;
        
        if (bind.ContinuousPress)
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

    public bool IsPressed(KeyBinds.Bind bind) => _pressed.Contains(bind) || _shortPressed.Contains(bind);
    public bool IsJustReleased(KeyBinds.Bind bind) => _justReleased.Contains(bind);
    
    public void ClearForNextFrame()
    {
        _shortPressed.Clear();
        _justReleased.Clear();
        DeltaCursorPosition = (0, 0);
    }
}