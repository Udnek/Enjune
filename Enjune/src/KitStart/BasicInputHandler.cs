using System.Diagnostics;
using Enjune.Graphic.Api;
using Enjune.Graphic.Key;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.KitStart;

public class BasicInputHandler : IUserInputHandler
{
    private readonly HashSet<KeyCode> _justPressed = [];
    private readonly HashSet<KeyCode> _pressed = [];
    private readonly HashSet<KeyCode> _justReleased = [];

    public readonly List<char> InputChars = [];
    
    // keys and mouse
    private bool _firstCursorMove = true;
    public Vector2i CursorPosition = (0, 0);
    public Vector2i DeltaCursorPosition { get; private set; } = (0, 0);
    public Vector2 DeltaWheelScroll { get; private set; } = (0, 0);
    public int MouseUpdates { get; private set; } // debug only

    // window 
    private readonly Stopwatch _lastWindowChange;
    private Vector2i _pendingWindowSize;
    private readonly Seconds _debouncingDelay;
    public Vector2i WindowSize { get; private set; }
    public bool WindowSizeChanged { get; private set; } = false;

    public BasicInputHandler(Vector2i initialWindowSize, Seconds debouncingDelay)
    {
        _debouncingDelay = debouncingDelay;
        WindowSize = initialWindowSize;
        _lastWindowChange = Stopwatch.StartNew();
    }

    public void PrepareAtFrameStart()
    {
        MouseUpdates = 0;
        
        if (_pendingWindowSize == default) return;
        // debouncing check
        if (_lastWindowChange.ElapsedMilliseconds < _debouncingDelay * 1000)
            return;
        WindowSizeChanged = true;
        WindowSize = _pendingWindowSize;
        _pendingWindowSize = default;
    }
    
    public bool IsJustPressed(KeyCode key) => _justPressed.Contains(key);
    public bool IsPressed(KeyBinds.Bind bind)
    {
        return bind.ContinuousPress ? _pressed.Contains(bind.KeyCode) : _justPressed.Contains(bind.KeyCode);
    }
    public bool IsJustReleased(KeyBinds.Bind bind) => _justReleased.Contains(bind.KeyCode);
    
    public void ClearForNextFrame()
    {
        _justPressed.Clear();
        _justReleased.Clear();
        InputChars.Clear();
        DeltaCursorPosition = (0, 0);
        DeltaWheelScroll = (0, 0);
        WindowSizeChanged = false;
    }
    
    //
    
    public void HandleWindowSizeChange(Vector2i newSize)
    {
        _pendingWindowSize = newSize;
        _lastWindowChange.Restart();
    }

    public void HandleKey(KeyCode key, IGraphicApi.KeyAction action)
    {
        switch (action)
        {
            case IGraphicApi.KeyAction.Press:
            case IGraphicApi.KeyAction.Repeat:
                _pressed.Add(key);
                _justPressed.Add(key);
                break;
            case IGraphicApi.KeyAction.Release:
                _pressed.Remove(key);
                _justReleased.Add(key);
                break;
        }
    }
    
    public void HandleCharacter(char character) => InputChars.Add(character);

    public void HandleCursorFromLeftBottom(int x, int y)
    {
        MouseUpdates++;
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

    public void HandleScroll(float x, float y)
    {
        DeltaWheelScroll += (x, y);
    }
}