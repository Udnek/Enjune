using Enjune.Graphic.Key;
using Enjune.KitStart;
using UiAddon.Element;

namespace UiAddon.Presets;

public class UiBasicButton : UiRect
{
    public UiBasicButton(UiElement[] children, Rect localAnchor, Margin margin, float globalZ, Color color, Action onClick) 
        : base(children, localAnchor, margin, globalZ, color)
    {
        OnClick = onClick;

        IsHovered.OnChange += (_, hovered)
            => Color.Val += new Vector4(0.4f, 0.4f, 0.4f, 0) * (hovered ? 1 : -1);
    }

    protected readonly Action OnClick;

    public override BeingHoveredAction UpdateBeingHovered(BasicInputHandler inputHandler) 
        => BeingHoveredAction.BecomeFocused;

    public override BeingFocusedAction UpdateBeingFocused(BasicInputHandler inputHandler)
    {
        if (inputHandler.IsJustPressed(KeyCode.LeftMouseButton)) 
            OnClick();
        return base.UpdateBeingFocused(inputHandler);
    }
}