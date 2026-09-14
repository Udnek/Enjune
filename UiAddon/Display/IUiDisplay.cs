using System.Collections.Specialized;
using Enjune.Attribute;
using Enjune.Graphic.Modeling;
using Enjune.Misc;
using UiAddon.Element;

namespace UiAddon.Display;

public interface IUiDisplay : IDisposable
{
    /// <summary>
    /// Meshes for Ui to be collected
    /// </summary>
    IList<Model.Entry> Meshes { get; }
    
    /// <summary>
    /// UiElement calls when this display removed from it to avoid memory leaks with ObservableValue subscription
    /// </summary>
    public void UnsubscribeFromParent();

    /// <summary>
    /// Called when is added to UiElement.
    /// Should be used initialize subscriptions
    /// </summary>
    public void Initialize();
}