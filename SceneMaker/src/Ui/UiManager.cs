using System.Diagnostics;
using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Key;
using Enjune.Misc;
using Enjune.World;
using SceneMaker.Misc;
using UiAddon;
using UiAddon.Element;
using UiAddon.Presets;

namespace SceneMaker.Ui;

public class UiManager : AbstractDisposable
{
    [DoNotDisposeViaUtils("would cause cycle disposing")]
    private readonly App _app;
    
    public readonly UiAddon.Element.Ui Ui;
    private readonly UiText _fps;
    private readonly UiRect _inspectorBackground;
    //private readonly UiText _text;
    private readonly UiRect _toggleVisibilityButton;
    private readonly UiDirectory _inspectorComps;
    private readonly KeyBinds.Bind _sizeChangeBind = new("ui_size_change", KeyCode.LeftCtrl, true);
    private readonly CompiledFont _font;

    public UiManager(App app, CompiledFont font)
    {
        _app = app;
        _font = font;

        _fps = new UiText(
            [],
            Anchor.FixedAt(0, 1),
            new Margin(0, 10, 10, -40),
            99,
            font, 
            "fps",
            Colors.UiText
        );
        
        _toggleVisibilityButton = new UiBasicButton(
            [],
            Anchor.FixedAt(0, 0.5f),
            new Margin(0, -40, -40, -40),
            1,
            Colors.Red,
            () => ToggleMenu(!_isMenuOpened)
        );
        _inspectorComps = new UiDirectory([]);
        _inspectorBackground = new UiRect(
            [_toggleVisibilityButton, _inspectorComps],
            Anchor.OfXy((1f, 1f), Anchor.Stretch),
            Margin.No,
            0,
            Colors.UiBackground
        );
        
        Ui = new UiAddon.Element.Ui(
            app.Grapi, app.InputHandler,
            [_fps, _inspectorBackground]
        );
        
        _inspectorBackground.LocalVisible.Val = false;
        _fps.LocalVisible.Val = true;
    }

    private bool _isMenuOpened = false;
    private void ToggleMenu(bool open)
    {
        if (!open) // closing
        {
            _isMenuOpened = false;
            _inspectorBackground.LocalAnchor.Val = Anchor.OfXy((1f, 1), Anchor.Stretch);
        }
        else // opening
        {
            _isMenuOpened = true;
            _inspectorBackground.LocalAnchor.Val = Anchor.OfXy((0.3f, 1), Anchor.Stretch);
        }
    }

    private readonly Stopwatch _fpsStopWatch = Stopwatch.StartNew();
    private readonly Remember<SObject?> _rememberSelectedObject = new(null);
    private readonly Remember<bool> _rememberUiFocused = false;

    public void Update(Seconds deltaTime)
    {
        var inputHandler = _app.InputHandler;
        var editControl = _app.EditorController;
        
        // global changes
        if (Ui.IsFocused && inputHandler.DeltaWheelScroll.Y != 0 && inputHandler.IsPressed(_sizeChangeBind))
        {
            Ui.PixelsPerUnit.Val += inputHandler.DeltaWheelScroll.Y * 0.1f;
        }
        
        Ui.Update();
        
        _rememberUiFocused.Val = Ui.IsFocused;
        _rememberSelectedObject.Val = editControl.SelectedObject;
        
        // pop up inspector
        if (_rememberSelectedObject.Changed)
        {
            if (_rememberSelectedObject.Val is null)
            {
                _inspectorBackground.LocalVisible.Val = false;
                ToggleMenu(false);
            }
            else
            {
                _inspectorBackground.LocalVisible.Val = true;
                
                // adding inputs
                _inspectorComps.Children.Clear();
                List<(string Name, float Val)> components = [("aboba", 42f), ("bebra", 52), ("kek", 123)]; //  
                const float elemYSize = 40f;
                const float betweenComp = 10f;
            
                float yOffset = betweenComp + elemYSize;
                foreach (var component in components)
                {
                    var nameElem = new UiText([],
                        Anchor.OfXy((0, 0), Anchor.Stretch),
                        Margin.No,
                        3,
                        _font,
                        component.Name,
                        Colors.UiText);

                    var valueElem = new UiEditableText([],
                        Anchor.OfXy((0.3f, 1f), Anchor.Stretch),
                        Margin.No,
                        3,
                        _font,
                        component.Val.ToString(),
                        Colors.UiText
                    );

                    var elem = new UiRect([nameElem, valueElem],
                        Anchor.OfXy(Anchor.Stretch, (1, 1)),
                        new Margin(betweenComp, -elemYSize / 2, betweenComp, -elemYSize / 2).Move(0, -yOffset),
                        2,
                        Colors.Blue
                    );    
   
                    yOffset += elemYSize+betweenComp;
                    _inspectorComps.Children.Add(elem);
                }
            }
        }

        if (inputHandler.IsJustPressed(KeyCode.F3))
            _fps.LocalVisible.Val = !_fps.LocalVisible;
        
        // fps counter
        if (_fps.LocalVisible && _fpsStopWatch.ElapsedMilliseconds > 1000 || _rememberUiFocused.Changed)
        {
            _fpsStopWatch.Restart();
            _fps.Text.Val = $"fps: {1f / deltaTime:0.00}; mouseUpdates: {inputHandler.MouseUpdates}; uiFocused: {Ui.FocusedElement};";
        }
        
    }

    protected override void DisposeData() => Utils.DisposeAllFields(this);
}