using System.Collections.Specialized;
using Enjune.Attribute;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using UiAddon.Element;

namespace UiAddon.Display;

public interface IUiDisplay
{
    IList<Model.Entry> Meshes { get; }
    void OnRectChange(Rect oldRect, Rect newRect);
    void OnHoverChange(bool hovered);
    void OnFocusChange(bool focused);
}