using UiAddon.Display;
using UiAddon.Display.Abstraction;
using UiAddon.Element;

namespace UiAddon.Theme;

public interface IUiTheme
{
    void ApplyToButton(ButtonElement button, string textOnButton);
    void ApplyBackground(IUiElement element);
    void ApplyToTextSign(TextElement element);
    void ApplyToEditableText(EditableTextElement element);
}