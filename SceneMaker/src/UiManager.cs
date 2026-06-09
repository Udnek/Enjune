using System.Diagnostics;
using Enjune.Graphic.Api;
using Enjune.Graphic.Asset.Font;
using Enjune.Graphic.Key;
using Enjune.Graphic.UI;
using Enjune.Misc;
using Enjune.World;

namespace SceneMaker;

public class UiManager : AbstractDisposable
{
    [DoNotAutoDispose("would cause cycle disposing")]
    private readonly App _app;
    
    public readonly IRenderableModel.IDynamic UiModel;
    public readonly Ui Ui;
    private readonly UiText _fpsUiElement;
    private readonly UiRect _uiObjectInfoBox;
    private readonly UiText _uiObjectText;

    public UiManager(App app, Vector2 initialWindowSize, CompiledFont font)
    {
        _app = app;
        
        _fpsUiElement = new UiText(Anchor.FixedAt(0, 1), new Margin(0, 10, 10, -40), 1, font, "fps", Colors.UiText);

        _uiObjectText = new UiText(Anchor.FixedAt(0, 1), new Margin(0, 0, 0, -40), 2, font, "", Colors.UiText);
        _uiObjectInfoBox = new UiRect(Anchor.OfXy((0.3f, 1f), Anchor.Stretch), Margin.No, 1, Colors.UiBackground, _uiObjectText)      
        {
            LocalHidden = true
        };
        Ui = new Ui(
            initialWindowSize,
            _fpsUiElement,
            _uiObjectInfoBox
        );
        
        UiModel = app.Grapi.CreateDynamicRenderable(Ui.CreateModel()); 
    }

    private readonly Stopwatch _fpsStopWatch = Stopwatch.StartNew();
    private SObject? _lastSelectedObject;
    public void Update(Seconds deltaTime, bool isFocused)
    { 
        var uiChanged = false;
        var inputHandler = _app.InputHandler;
        var editControl = _app.EditorController;
        
        // global changes
        if (inputHandler.DeltaWheelScroll.Y != 0 && isFocused)
        {
            uiChanged = true;
            Ui.PixelsPerUnit += inputHandler.DeltaWheelScroll.Y * 0.1f;
            Ui.UpdateAllRectsAndVisibleMeshes();
        }
        if (inputHandler.WindowSizeChanged)
        {
            Ui.Size = inputHandler.WindowSize;
            Ui.UpdateAllRectsAndVisibleMeshes();
            uiChanged = true;
        }
        
        // pop up inspector
        if (editControl.SelectedObject != _lastSelectedObject)
        {
            _lastSelectedObject = editControl.SelectedObject;
            if (_lastSelectedObject is not null)
            {
                _uiObjectText.Text = "text";
                _uiObjectInfoBox.LocalHidden = false;
            }
            else
                _uiObjectInfoBox.LocalHidden = true;

            _uiObjectInfoBox.RegenerateSelfAndVisibleChildrenMeshes();
            uiChanged = true;
        }
        
        Ui.RecheckHoveredElements(inputHandler.CursorPosition);
        
        // inspector input
        if (_lastSelectedObject is not null && isFocused)
        {
            var enterPressed = inputHandler.IsJustPressed(KeyCode.Enter);
            var backspacePressed = inputHandler.IsJustPressed(KeyCode.Backspace);
            if (enterPressed || backspacePressed || inputHandler.InputChars.Count > 0)
            {
                if (enterPressed) 
                    inputHandler.InputChars.Add('\n');
            
                _uiObjectText.Text += new string(inputHandler.InputChars.AsSpan());
            
                if (backspacePressed && _uiObjectText.Text.Length != 0) 
                    _uiObjectText.Text = _uiObjectText.Text[..^1];
            
                _uiObjectText.RegenerateSelfMeshes();
                uiChanged = true;
            }
        }
        
        // fps counter
        if (_fpsStopWatch.ElapsedMilliseconds > 1000)
        {
            _fpsStopWatch.Restart();
            _fpsUiElement.Text = $"mi: {inputHandler.MouseUpdates}; fps: {1f / deltaTime:0.00}";
            _fpsUiElement.RegenerateSelfMeshes();
            uiChanged = true;
        }
        
        // model update
        if (uiChanged){
            UiModel.Refit(Ui.CreateModel());
        }
    }

    protected override void DisposeData() => Utils.DisposeAllFields(this);
}