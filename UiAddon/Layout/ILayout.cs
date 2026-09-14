using Enjune.Attribute;
using Enjune.Misc;
using JetBrains.Annotations;
using UiAddon.Element;

namespace UiAddon.Layout;

public interface ILayout
{
    /// <summary>
    /// Updates children rect and returns new layout (self) for caller
    /// </summary>
    /// <param name="selfRect"></param>
    /// <param name="allChildren"></param>
    /// <returns></returns>
    [MustUseReturnValue]
    ILayout UpdateSelfLayoutAndChildrenRects(Rect selfRect, IList<IUiElement> allChildren);
}
