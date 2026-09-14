using Enjune.Graphic.Key;
using Enjune.KitStart;
using Enjune.Misc;
using UiAddon.Display;
using UiAddon.Layout;

namespace UiAddon.Element;

public class ButtonElement : AbstractUiElement
{
    // mostly for Displays to observe
    public readonly ObservableValue<int> Clicks = 0;
    
    public ButtonElement(Action onClick, ILayout layout, float globalZ = 0, IEnumerable<IUiElement>? children = null) 
        : base(layout, globalZ, children)
    {
        OnClick = onClick;
    }

    public Action OnClick;

    public override IUiElement.BeingFocusedAction UpdateBeingFocused(BasicInputHandler inputHandler)
    {
        if (inputHandler.IsJustPressed(KeyCode.LeftMouseButton))
        {
            OnClick();
            Clicks.Val += 1;
        }
        return base.UpdateBeingFocused(inputHandler);
    }
}