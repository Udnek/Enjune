using Enjune.Misc;
using UiAddon.Element;

namespace UiAddon.Layout;

public readonly record struct AnchorData : ILayout
{
    public static readonly AnchorData FullFill = new AnchorData
    {
        Anchor = UiAddon.Anchor.FullStretch,
        Margin = Margin.No
    };

    public required Rect Anchor { get; init; }
    public required Margin Margin { get; init; }
    
    public ILayout UpdateSelfLayoutAndChildrenRects(Rect selfRect, IList<IUiElement> allChildren)
    {
        foreach (var child in allChildren)
        {
            if (child.Layout is not AnchorData childAnchor)
            {
                Logger.Warn(this, $"Child {child} has incompatible layout {child.Layout}, expected {typeof(AnchorData)}");
                continue;
            }
            child.Rect.Val = UiAddon.Anchor.CalculateRectFromParent(selfRect, childAnchor.Anchor, childAnchor.Margin);
        }

        return this;
    }
}