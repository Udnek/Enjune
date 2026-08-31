namespace UiAddon.Element;

public class UiDirectory(UiElement[] children, Rect localAnchor, Margin margin)
    : UiElement(children, localAnchor, margin, 0)
{
    public UiDirectory(UiElement[] children) : this(children, Anchor.FullStretch, UiAddon.Margin.No)
    {
    } 

    protected override void UpdateShape(Rect oldValue, Rect newValue) { }
}