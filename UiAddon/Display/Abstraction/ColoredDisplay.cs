using Enjune.Misc;
using UiAddon.Element;

namespace UiAddon.Display.Abstraction;

public abstract class ColoredDisplay<TParent> : AbstractUiDisplay<TParent> where TParent : IUiElement
{
    public required ObservableValue<Color> Color { get; init; }

    protected ColoredDisplay()
    {
    }
    
    public override void Initialize()
    {
        Color.ObserveAsOwner((_, _) => OnColorChange());
        base.Initialize();
    }

    private void OnColorChange()
    {
        for (var i = 0; i < Meshes.Count; i++) 
            Meshes[i] = Meshes[i].WithColor(Color);
    }
}