using Enjune.Graphic.GraphicApi;
using Enjune.Misc;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Enjune.Graphic.InputHandler;

public class BasicInputHandler : IUserInputHandler
{
    private readonly IGraphicApi _graphicApi;
    public readonly KeyBinds _binds;
    private readonly HashSet<KeyBinds.Bind> _pressed = [];
    private readonly HashSet<KeyBinds.Bind> _shortPressed = [];
    private bool _firstCursorMove = true;
    public Vector2i MousePosition = (0, 0);
    public Vector2i DeltaMousePosition { private set; get; } = (0, 0);
    //public Vector2i MousePosition { private set; get; }= (0, 0);

    public BasicInputHandler(IGraphicApi graphicApi, KeyBinds binds)
    {
        _graphicApi = graphicApi;
        _binds = binds;
    }

    public void HandleKey(GlfwKey key, IGraphicApi.KeyAction action) => Handle(UniKey.Of(key), action);
    
    public void HandleMouseKey(MouseButton key, IGraphicApi.KeyAction action) => Handle(UniKey.Of(key), action);

    private void Handle(UniKey uniKey, IGraphicApi.KeyAction action)
    {
        if (!_binds.TryGet(uniKey, out var bind))
            return;
        
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

    public void HandleCursor(int x, int y)
    {
        if (_firstCursorMove)
        {
            MousePosition = (x, y);
            _firstCursorMove = false;
            return;
        }
        // we += cause this function will be called several times between frames
        DeltaMousePosition += (x, y) - MousePosition;
        MousePosition = (x, y);
    }

    public bool IsPressed(KeyBinds.Bind bind) => _pressed.Contains(bind) || _shortPressed.Contains(bind);

    public void ClearForNextFrame()
    {
        _shortPressed.Clear();
        DeltaMousePosition = (0, 0);
    }
}