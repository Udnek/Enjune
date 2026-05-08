using System.Diagnostics;
using Enjune.Graphic.Api;
using Enjune.Graphic.Key;
using Enjune.Misc;
using OpenTK.Mathematics;

namespace Enjune.KitStart;

public class BasicInputHandler : IUserInputHandler
{
    private readonly Mutex _mutex = new();
    
    public readonly KeyBinds Binds;

    private readonly HashSet<KeyBinds.Bind> _pressed = [];
    private readonly HashSet<KeyBinds.Bind> _shortPressed = [];
    private readonly HashSet<KeyBinds.Bind> _justReleased = [];
    
    // keys and mouse
    private bool _firstCursorMove = true;
    public Vector2i CursorPosition = (0, 0);
    public Vector2i DeltaCursorPosition { get; private set; } = (0, 0);
    public Vector2 DeltaWheelScroll { get; private set; } = (0, 0);
    
    // window 
    private readonly Stopwatch _lastWindowChange;
    private Vector2i _pendingWindowSize;
    private readonly Seconds _debouncingDelay;
    public Vector2i WindowSize { get; private set; }
    public bool WindowSizeChanged { get; private set; } = false;

    public BasicInputHandler(KeyBinds binds, Vector2i initialWindowSize, Seconds debouncingDelay)
    {
        Binds = binds;
        _debouncingDelay = debouncingDelay;
        WindowSize = initialWindowSize;
        _lastWindowChange = Stopwatch.StartNew();
    }

    public void HandleWindowSizeChange(Vector2i newSize)
    {
        _mutex.Lock(() => 
        {
            _pendingWindowSize = newSize;
            _lastWindowChange.Restart();
        });
    }

    public void HandleKey(KeyCode keyCode, IGraphicApi.KeyAction action)
    {
        _mutex.Lock(() =>
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
            
            
        });
    }

    public void HandleCursor(int x, int y)
    {
        _mutex.Lock(() =>
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
        });
    }

    public void HandleScroll(float x, float y)
    {
        _mutex.Lock(() =>
        {
            DeltaWheelScroll += (x, y);
        });
    }

    public bool IsPressed(KeyBinds.Bind bind) => _pressed.Contains(bind) || _shortPressed.Contains(bind);
    public bool IsJustReleased(KeyBinds.Bind bind) => _justReleased.Contains(bind);

    public void PrepareAtFrameStart()
    {
        // mutex lock
        _mutex.WaitOne();
        
        if (_pendingWindowSize == default) return;
        // debouncing check
        if (_lastWindowChange.ElapsedMilliseconds < _debouncingDelay * 1000)
            return;
        WindowSizeChanged = true;
        WindowSize = _pendingWindowSize;
        _pendingWindowSize = default;
    }
    
    public void ClearForNextFrame()
    {
        _shortPressed.Clear();
        _justReleased.Clear();
        DeltaCursorPosition = (0, 0);
        DeltaWheelScroll = (0, 0);
        WindowSizeChanged = false;
        
        // mutex unlock
        _mutex.ReleaseMutex();
    }
}