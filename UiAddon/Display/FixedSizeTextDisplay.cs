using Enjune.Graphic.Asset.Font;
using Enjune.Misc;
using UiAddon.Display.Abstraction;
using UiAddon.Element;
using UiAddon.Layout;

namespace UiAddon.Display;

public class FixedSizeTextDisplay : StaticTextDisplay
{
    public required ObservableValue<float> TextHeight { get; init; }

    public override void Initialize()
    {
        TextHeight.ObserveAsOwner((_, _) => RegenerateMeshes());
        base.Initialize();
    }

    protected override void RegenerateMeshes()
    {
        Meshes.Clear();
        CreateMeshes(Parent.TextLines, Font, TextHeight, Align, m => Meshes.Add(m.Mesh));
    }


    protected override void OnRectChange(Rect oldRect, Rect newRect) => RegenerateMeshes();
}