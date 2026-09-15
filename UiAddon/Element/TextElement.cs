using Enjune.Misc;
using UiAddon.Layout;

namespace UiAddon.Element;

public class TextElement : AbstractUiElement
{
    public readonly ObservableValue<string> Text;
    
    public TextElement(string text, ILayout layout, float globalZ = 0, IEnumerable<IUiElement>? children = null) 
        : base(layout, globalZ, children)
    {
        Text = text;
    }
}