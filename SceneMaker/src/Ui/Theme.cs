using Enjune.Graphic.Asset.Font;
using SceneMaker.Misc;
using UiAddon.Display;
using UiAddon.Display.Abstraction;
using UiAddon.Element;
using UiAddon.Theme;

namespace SceneMaker.Ui;

public class Theme(CompiledFont font) : IUiTheme
{
    public void ApplyToButton(ButtonElement button, string textOnButton)
    {
        button.Displays.Add(new RectDisplay
        {
            Parent = button,
            Color = Colors.Red
        });
    }

    public void ApplyBackground(IUiElement element)
    {
        element.Displays.Add(new RectDisplay
        {
            Parent = element,
            Color = new Vector4(Random.Shared.Next(0, 255)/255f,Random.Shared.Next(0, 255)/255f,Random.Shared.Next(0, 255)/255f,0.5f)
        });
    }

    public void ApplyToTextSign(TextElement element)
    {
        ApplyBackground(element);
        element.Displays.Add(new SizeAdjustingTextDisplay
        {
            Parent = element,
            Color = Colors.UiText,
            Font = font,
            ZOffset = 1
        });
    }

    public void ApplyToEditableText(EditableTextElement element)
    {
        element.Displays.Add(new RectDisplay
        {
            Parent = element,
            Color = Colors.Green
        });
        element.Displays.Add(new EditableTextDisplay()
        {
            Parent = element,
            Color = Colors.UiText,
            ZOffset = 1
        });
    }
}