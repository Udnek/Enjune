using Enjune.Graphic.Key;
using Enjune.KitStart;
using UiAddon.Display;
using UiAddon.Element;

namespace UiAddon.Presets;

public class ButtonElement : UiElement
{
    public ButtonElement(UiElement[] children, UiDisplay[] displays, Rect localAnchor, Margin margin, float globalZ, Action onClick) 
        : base(children, displays, localAnchor, margin, globalZ)
    {
        OnClick = onClick;
    }

    protected readonly Action OnClick;

    public override BeingFocusedAction UpdateBeingFocused(BasicInputHandler inputHandler)
    {
        if (inputHandler.IsJustPressed(KeyCode.LeftMouseButton)) 
            OnClick();
        return base.UpdateBeingFocused(inputHandler);
    }
}