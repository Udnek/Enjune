namespace Enjune.Graphic.KeyHandler;

class BasicKeyHandler: IUserInputHandler
{
    private readonly ISet<KeyBinds.Bind> _pressed = new HashSet<KeyBinds.Bind>();
    private readonly ISet<KeyBinds.Bind> _shortPressed = new HashSet<KeyBinds.Bind>();
    
    public void Handle(GlfwKey key, IGraphicApi.KeyAction action)
    {
        var bind = KeyBinds.Get(key);
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


    public bool IsPressed(KeyBinds.Bind bind) => _pressed.Contains(bind) || _shortPressed.Contains(bind);

    public void ClearForNextFrame() => _shortPressed.Clear();
}