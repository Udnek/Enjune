using Enjune.Misc;
using UiAddon.Layout;

namespace UiAddon.Element;

public class extElement<TSelfLayout> : AnchorDirectoryElement<TSelfLayout> where TSelfLayout : ILayoutData
{
    public readonly ObservableList<string> TextLines;
    
    public extElement(IEnumerable<string> textLines, TSelfLayout layout, float globalZ, IEnumerable<IUiElement<AnchorData>> children) : base(layout, globalZ, children)
    {
        TextLines = new ObservableList<string>(textLines);
    }
}