using Enjune.Misc;
using UiAddon.Element;

namespace UiAddon.Layout;

public readonly record struct AnchorLayout : ILayout
{
    public static readonly AnchorLayout FullFill = new AnchorLayout
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
            if (child.Layout.Val is not AnchorLayout childAnchor)
            {
                Logger.Warn(this, $"Child {child} has incompatible layout {child.Layout}, expected {typeof(AnchorLayout)}");
                continue;
            }
            child.SetRectAsParent(UiAddon.Anchor.CalculateRectFromParent(selfRect, childAnchor.Anchor, childAnchor.Margin));
        }

        return this;
    }
}