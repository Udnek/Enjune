using Enjune.Graphic.Asset.Font;
using Enjune.Misc;
using UiAddon.Element;
using UiAddon.Layout;

namespace UiAddon.Display.Abstraction;

public abstract class StaticTextDisplay : TextDisplay<TextElement>
{
    public required ObservableValue<CompiledFont> Font { get; init; }
    public ObservableValue<Alignment> Align { get; init; } = Alignment.Center;

    public override void Initialize()
    {
        Font.ObserveAsOwner((_, _) => RegenerateMeshes());
        Align.ObserveAsOwner((_, _) => RegenerateMeshes());
        base.Initialize();
    }

    protected override void OnRectChange(Rect oldRect, Rect rect) => RegenerateMeshes();
}