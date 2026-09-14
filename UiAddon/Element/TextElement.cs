using Enjune.Misc;
using UiAddon.Layout;

namespace UiAddon.Element;

public class TextElement : AbstractUiElement
{
    public readonly ObservableList<string> TextLines;
    
    public TextElement(IEnumerable<string> textLines, ILayout layout, float globalZ = 0, IEnumerable<IUiElement>? children = null) 
        : base(layout, globalZ, children)
    {
        TextLines = new ObservableList<string>(textLines);
    }
}