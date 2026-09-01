using System.Diagnostics;
using Enjune.Attribute;
using Enjune.Data.Json;
using Enjune.Ecs.EcsType;
using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Key;
using Enjune.Misc;
using Enjune.Registering;
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
            20,
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
            app.GraphicApi, app.InputHandler,
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
    private readonly Remember<Entity?> _rememberSelectedObject = new(null);
    private readonly Remember<bool> _rememberUiFocused = false;

    public void Update(Seconds deltaTime)
    {
        var inputHandler = _app.InputHandler;
        var editControl = _app.EditorSystem;
        
        // global changes
        if (Ui.IsFocused && inputHandler.DeltaWheelScroll.Y != 0 && inputHandler.IsPressed(_sizeChangeBind))
        {
            Ui.PixelsPerUnit.Val += inputHandler.DeltaWheelScroll.Y * 0.1f;
        }
        
        Ui.Update();
        
        _rememberUiFocused.Val = Ui.IsFocused;
        _rememberSelectedObject.Val = editControl.SelectedEntity;
        
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
                List<(string Name, string Content)> components = [];
                var entity = _app.EditorSystem.SelectedEntity;
                if (entity is null) 
                    components.Add(("nothing selected", ":("));
                else
                {
                    foreach (var component in _app.World.GetEntityComponents(entity.Value))
                    {
                        var compId = component.Id();
                        var codec = Registries.Codec.Get(compId, out var getCodecErr);
                        if (codec is null)
                        {
                            var errTxt = $"Codec for {compId} not found: {getCodecErr}";
                            Logger.Warn(this, errTxt);
                            components.Add((compId.ToString(), errTxt));
                            continue;
                        }

                        var resultOrError = codec.EncodeObj(component);
                        resultOrError.Map(data =>
                        {
                            components.Add((compId.ToString(), JsonSerde.Indent4.Serialize(data)));
                        },
                        err =>
                        {
                            var errTxt = $"Can not encode component {component}: {err}";
                            Logger.Warn(this, errTxt);
                            components.Add((compId.ToString(), errTxt));
                        });
                    }
                }
                
                const float previousYSize = 0f;
                const float betweenComp = 10f;
            
                float yOffset = betweenComp + previousYSize;
                foreach (var component in components)
                {
                    var ySize = (component.Content.Count('\n') + 1) * 40f;
                    
                    var nameElem = new UiText([],
                        Anchor.FixedAt(0, 1),
                        Margin.No,
                        3,
                        _font,
                        component.Name,
                        20,
                        Colors.UiText);

                    var contentElem = new UiEditableText([],
                        Anchor.OfXy((0.3f, 1f), (1, 1)),
                        Margin.No,
                        3,
                        _font,
                        component.Content,
                        20,
                        Colors.UiText
                    );

                    var elem = new UiRect([nameElem, contentElem],
                        Anchor.OfXy(Anchor.Stretch, (1f, 1f)),
                        new Margin(
                            10, -ySize / 2,
                            10, -ySize / 2)
                            .Move(0, -yOffset),
                        2,
                        Colors.Blue
                    );    
   
                    yOffset += ySize+betweenComp;
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