using System.Net.Mime;
using Enjune.Graphic.Asset.Font;
using Enjune.Misc;
using UiAddon.Display.Abstraction;
using UiAddon.Element;
using UiAddon.Layout;

namespace UiAddon.Display;

public class SizeAdjustingTextDisplay : StaticTextDisplay
{
    private readonly ObservableValue<float> _textHeight = 1;

    public override void Initialize()
    {
        _textHeight.ObserveAsOwner((_, _) => RegenerateMeshes());
        base.Initialize();
    }

    protected override void RegenerateMeshes()
    {
        Meshes.Clear();
        CreateMeshes(Parent.Text.Val.Split('\n'), Font, _textHeight, Align, m => Meshes.Add(m.Mesh));
    }

    protected override void OnRectChange(Rect oldRect, Rect newRect)
    {
        _textHeight.Val = newRect.Height / Parent.Text.Val.Count('\n');
    }
}