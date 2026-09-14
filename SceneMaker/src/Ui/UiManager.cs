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
using UiAddon.Layout;

namespace SceneMaker.Ui;

public class UiManager : AbstractDisposable
{
    [DoNotDisposeViaUtils("would cause cycle disposing")]
    private readonly App _app;
    
    public readonly UiAddon.Ui Ui;
    private readonly KeyBinds.Bind _sizeChangeBind = new("ui_size_change", KeyCode.LeftCtrl, true);
    private readonly CompiledFont _font;

    public UiManager(App app, CompiledFont font)
    {
        _app = app;
        _font = font;
        var theme = new Theme(font);

        Ui = new UiAddon.Ui(_app.GraphicApi, _app.InputHandler, 
        [
            
            new EditableTextElement(
                ["Line 0", "Line 1", "Line 2"],
                font,
                40,
                new FlexBoxLayout()
                {
                    
                }
                )
        ]);
        
        foreach (var root in Ui.Roots)
        {
            ApplyBack(root);
        }

        void ApplyBack(IUiElement element)
        {
            if (element is EditableTextElement editableTextElement)
                theme.ApplyToEditableText(editableTextElement);
            else if (element is TextElement textElement)
                theme.ApplyToTextSign(textElement);
            else
                theme.ApplyBackground(element);
            element.Children.ForEach(ApplyBack);
        }
    }

    public void Update(Seconds deltaTime)
    {
        var inputHandler = _app.InputHandler;
        
        // global changes
        if (inputHandler.DeltaWheelScroll.Y != 0 && inputHandler.IsPressed(_sizeChangeBind))
        {
            Ui.PixelsPerUnit.Val += inputHandler.DeltaWheelScroll.Y * 0.1f;
        }
        
        Ui.Update(deltaTime);
        
    }

    protected override void DisposeData() => Utils.DisposeAllFields(this);
}